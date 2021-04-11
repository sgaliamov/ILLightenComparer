using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ILLightenComparer.Comparer;
using ILLightenComparer.Config;
using ILLightenComparer.Equality;
using ILLightenComparer.Shared;

[assembly: InternalsVisibleTo("IL-Lighten-Comparer.dll")]

namespace ILLightenComparer
{
    /// <summary>
    ///     Implements the API to configure and generate comparers.
    /// </summary>
    public sealed class ComparerBuilder : IComparerBuilder
    {
        public static IComparerBuilder Default { get; } = new ComparerBuilder();
        private readonly ConfigurationProvider _configurationProvider = new();
        private Lazy<(ComparerContext, EqualityContext)> _contexts;

        public ComparerBuilder()
        {
            InitContext();
        }

        public ComparerBuilder(Action<IConfigurationBuilder> config) : this()
        {
            Configure(config);
        }

        public IComparerBuilder Configure(Action<IConfigurationBuilder> config)
        {
            config(_configurationProvider);

            InitContext();

            return this;
        }

        public IComparerBuilder<T> For<T>() => new Proxy<T>(this);

        public IComparerBuilder<T> For<T>(Action<IConfigurationBuilder<T>> config) => new Proxy<T>(this).Configure(config);

        public IComparer<T> GetComparer<T>() => _contexts.Value.Item1.GetComparer<T>();

        public IEqualityComparer<T> GetEqualityComparer<T>() => _contexts.Value.Item2.GetEqualityComparer<T>();

        private void InitContext()
        {
            _contexts = new Lazy<(ComparerContext, EqualityContext)>(() => {
                var configurationCopy = new ConfigurationProvider(_configurationProvider);
                var membersProvider = new MembersProvider(configurationCopy);

                var comparerContext = new ComparerContext(membersProvider, configurationCopy);
                var equalityContext = new EqualityContext(comparerContext, membersProvider, configurationCopy);

                return (comparerContext, equalityContext);
            }, LazyThreadSafetyMode.PublicationOnly);
        }

        private sealed class Proxy<T> : IComparerBuilder<T>
        {
            private readonly ComparerBuilder _subject;

            public Proxy(ComparerBuilder subject)
            {
                _subject = subject;
            }

            public IComparerBuilder Builder => _subject;

            public IComparerBuilder<T> Configure(Action<IConfigurationBuilder<T>> config)
            {
                _subject._configurationProvider.ConfigureFor(config);

                _subject.InitContext(); // todo: 3. test init order

                return this;
            }

            public IComparerBuilder<TOther> For<TOther>() => _subject.For<TOther>();

            public IComparerBuilder<TOther> For<TOther>(Action<IConfigurationBuilder<TOther>> config) => _subject.For(config);

            public IComparer<T> GetComparer() => _subject.GetComparer<T>();

            public IComparer<TOther> GetComparer<TOther>() => _subject.GetComparer<TOther>();

            public IEqualityComparer<T> GetEqualityComparer() => _subject.GetEqualityComparer<T>();

            public IEqualityComparer<TOther> GetEqualityComparer<TOther>() => _subject.GetEqualityComparer<TOther>();
        }
    }
}
