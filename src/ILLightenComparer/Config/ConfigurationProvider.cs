using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ILLightenComparer.Extensions;
using ILLightenComparer.Shared;

namespace ILLightenComparer.Config
{
    using ConfigurationsCollection = ConcurrentDictionary<Type, Configuration>;

    internal interface IConfigurationProvider
    {
        Configuration Get(Type type);
        IComparer<T> GetCustomComparer<T>();
        IEqualityComparer<T> GetCustomEqualityComparer<T>();
        bool HasCustomComparer(Type type);
        bool HasCustomEqualityComparer(Type type);
    }

    internal sealed class ConfigurationProvider : IConfigurationBuilder, IConfigurationProvider
    {
        private const bool IncludeFieldsDefault = true;
        private const StringComparison StringComparisonTypeDefault = StringComparison.Ordinal;
        private const bool DetectCyclesDefault = true;
        private const bool IgnoreCollectionOrderDefault = false;
        private const long HashSeedDefault = 0x1505L; // https://github.com/dotnet/runtime/blob/e3ffd343ad5bd3a999cb9515f59e6e7a777b2c34/src/libraries/Common/src/Extensions/HashCodeCombiner/HashCodeCombiner.cs
        private static readonly string[] IgnoredMembersDefault = Array.Empty<string>();
        private static readonly string[] MembersOrderDefault = Array.Empty<string>();

        private static (Type, TComparer) CreateCustomComparer<TComparer>(Type genericInterface)
        {
            var comparerType = typeof(TComparer);
            var typedGenericInterface = comparerType.FindGenericInterface(genericInterface);
            if (genericInterface == null) {
                throw new ArgumentException($"{nameof(TComparer)} is not generic.");
            }

            var objectType = typedGenericInterface.GenericTypeArguments[0];
            var comparer = comparerType.Create<TComparer>();

            return (objectType, comparer);
        }

        private static string[] GetMembers<T, TMember>(IEnumerable<Expression<Func<T, TMember>>> memberSelectors) => memberSelectors?.Select(GetMemberName).ToArray();

        private static string GetMemberName<T, TMember>(Expression<Func<T, TMember>> selector)
        {
            if (selector.Body.NodeType != ExpressionType.MemberAccess) {
                throw new ArgumentException("Member selector is expected.", nameof(selector));
            }

            var body = (MemberExpression)selector.Body;

            return body.Member.Name;
        }

        private readonly ConfigurationsCollection _configuration;
        private readonly ComparersCollection _customComparers;
        private readonly ComparersCollection _customEqualityComparers;

        private readonly Configuration _default = new(
            IgnoredMembersDefault,
            IncludeFieldsDefault,
            MembersOrderDefault,
            StringComparisonTypeDefault,
            DetectCyclesDefault,
            IgnoreCollectionOrderDefault,
            HashSeedDefault);

        public ConfigurationProvider()
        {
            _configuration = new ConfigurationsCollection();
            _customComparers = new ComparersCollection();
            _customEqualityComparers = new ComparersCollection();
        }

        public ConfigurationProvider(ConfigurationProvider provider)
        {
            _configuration = new ConfigurationsCollection(provider._configuration.ToDictionary(x => x.Key, x => new Configuration(x.Value)));
            _customComparers = new ComparersCollection(provider._customComparers);
            _customEqualityComparers = new ComparersCollection(provider._customEqualityComparers);
            _default = new Configuration(provider._default);
        }

        public IConfigurationBuilder<T> ConfigureFor<T>(Action<IConfigurationBuilder<T>> config)
        {
            var proxy = new Proxy<T>(this);

            config(proxy);

            return proxy;
        }

        public IConfigurationBuilder DefineMembersOrder<T>(Action<IMembersOrder<T>> order)
        {
            if (order == null) {
                GetOrCreate(typeof(T)).SetMembersOrder(_default.MembersOrder);

                return this;
            }

            var members = new MembersOrder<T>();

            order(members);

            GetOrCreate(typeof(T)).SetMembersOrder(members.Order);

            return this;
        }

        public IConfigurationBuilder DetectCycles(Type type, bool? value)
        {
            GetOrCreate(type).DetectCycles = value ?? _default.DetectCycles;

            return this;
        }

        public Configuration Get(Type type) =>
            _configuration.TryGetValue(type, out var configuration)
                ? configuration
                : _default;

        public IComparer<T> GetCustomComparer<T>() =>
            _customComparers.TryGetValue(typeof(T), out var comparer) ? (IComparer<T>)comparer : null;

        public IEqualityComparer<T> GetCustomEqualityComparer<T>() =>
            _customEqualityComparers.TryGetValue(typeof(T), out var comparer)
                ? (IEqualityComparer<T>)comparer
                : null;

        private Configuration GetOrCreate(Type type) => _configuration.GetOrAdd(type, _ => new Configuration(_default));

        public bool HasCustomComparer(Type type) => _customComparers.ContainsKey(type);

        public bool HasCustomEqualityComparer(Type type) => _customEqualityComparers.ContainsKey(type);

        public IConfigurationBuilder IgnoreCollectionsOrder(Type type, bool? value)
        {
            GetOrCreate(type).IgnoreCollectionOrder = value ?? _default.IgnoreCollectionOrder;

            return this;
        }

        public IConfigurationBuilder IgnoreMember<T, TMember>(Expression<Func<T, TMember>>[] selectors)
        {
            var members = GetMembers(selectors);

            GetOrCreate(typeof(T)).SetIgnoredMembers(members);

            return this;
        }

        public IConfigurationBuilder IncludeFields(Type type, bool? value)
        {
            GetOrCreate(type).IncludeFields = value ?? _default.IncludeFields;

            return this;
        }

        public IConfigurationBuilder SetCustomComparer<T>(IComparer<T> instance) =>
            SetCustomComparer(_customComparers, typeof(T), instance);

        public IConfigurationBuilder SetCustomComparer<TComparer>()
        {
            var (type, comparer) = CreateCustomComparer<TComparer>(typeof(IComparer<>));

            return SetCustomComparer(_customComparers, type, comparer);
        }

        private ConfigurationProvider SetCustomComparer(ComparersCollection customComparers, Type objectType, object instance)
        {
            if (instance == null) {
                customComparers.TryRemove(objectType, out _);
            } else {
                customComparers.AddOrUpdate(objectType, _ => instance, (_, _) => instance);
            }

            return this;
        }

        public IConfigurationBuilder SetCustomEqualityComparer<T>(IEqualityComparer<T> instance) =>
            SetCustomComparer(_customEqualityComparers, typeof(T), instance);

        public IConfigurationBuilder SetCustomEqualityComparer<TComparer>()
        {
            var (type, comparer) = CreateCustomComparer<TComparer>(typeof(IEqualityComparer<>));

            return SetCustomComparer(_customEqualityComparers, type, comparer);
        }

        public IConfigurationBuilder SetDefaultCollectionsOrderIgnoring(bool? value)
        {
            _default.IgnoreCollectionOrder = value ?? IgnoreCollectionOrderDefault;

            return this;
        }

        public IConfigurationBuilder SetDefaultCyclesDetection(bool? value)
        {
            _default.DetectCycles = value ?? DetectCyclesDefault;

            return this;
        }

        public IConfigurationBuilder SetDefaultFieldsInclusion(bool? value)
        {
            _default.IncludeFields = value ?? IncludeFieldsDefault;

            return this;
        }

        public IConfigurationBuilder SetDefaultHashSeed(long? value)
        {
            _default.HashSeed = value ?? HashSeedDefault;

            return this;
        }

        public IConfigurationBuilder SetDefaultStringComparisonType(StringComparison? value)
        {
            _default.StringComparisonType = value ?? StringComparisonTypeDefault;

            return this;
        }

        public IConfigurationBuilder SetHashSeed(Type type, long? value)
        {
            GetOrCreate(type).HashSeed = value ?? _default.HashSeed;

            return this;
        }

        public IConfigurationBuilder SetStringComparisonType(Type type, StringComparison? value)
        {
            GetOrCreate(type).StringComparisonType = value ?? _default.StringComparisonType;

            return this;
        }

        private sealed class MembersOrder<T> : IMembersOrder<T>
        {
            private readonly List<string> _order = new();

            public IEnumerable<string> Order => _order;

            public IMembersOrder<T> Member<TMember>(Expression<Func<T, TMember>> selector)
            {
                _order.Add(GetMemberName(selector));

                return this;
            }
        }

        private sealed class Proxy<T> : IConfigurationBuilder<T>
        {
            private readonly ConfigurationProvider _subject;

            public Proxy(ConfigurationProvider subject)
            {
                _subject = subject;
            }

            public IConfigurationBuilder<T> DefineMembersOrder(Action<IMembersOrder<T>> order)
            {
                _subject.DefineMembersOrder(order);

                return this;
            }

            public IConfigurationBuilder<T> DetectCycles(bool? value)
            {
                _subject.DetectCycles(typeof(T), value);

                return this;
            }

            public IConfigurationBuilder<T> IgnoreCollectionsOrder(bool? value)
            {
                _subject.IgnoreCollectionsOrder(typeof(T), value);

                return this;
            }

            public IConfigurationBuilder<T> IgnoreMember<TMember>(Expression<Func<T, TMember>>[] selectors)
            {
                _subject.IgnoreMember(selectors);

                return this;
            }

            public IConfigurationBuilder<T> IncludeFields(bool? value)
            {
                _subject.IncludeFields(typeof(T), value);

                return this;
            }

            public IConfigurationBuilder<T> SetStringComparisonType(StringComparison? value)
            {
                _subject.SetStringComparisonType(typeof(T), value);

                return this;
            }
        }
    }
}
