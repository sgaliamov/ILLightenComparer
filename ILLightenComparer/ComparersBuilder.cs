﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Config;
using ILLightenComparer.Emit;
using ILLightenComparer.Emit.Extensions;

namespace ILLightenComparer
{
    public sealed class ComparersBuilder : IComparersBuilder
    {
        private readonly ConcurrentDictionary<Type, IComparer> _comparers = new ConcurrentDictionary<Type, IComparer>();
        private readonly ConfigurationBuilder _configurations = new ConfigurationBuilder();
        private readonly ComparerContext _context;

        public ComparersBuilder()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("ILLightenComparer"),
                AssemblyBuilderAccess.RunAndCollect);

            var moduleBuilder = assembly.DefineDynamicModule("ILLightenComparer.dll");

            _context = new ComparerContext(moduleBuilder, _configurations);
        }

        public IContextBuilder DefineDefaultConfiguration(ComparerSettings settings)
        {
            _configurations.DefineDefaultConfiguration(settings);
            return this;
        }

        public IContextBuilder DefineConfiguration(Type type, ComparerSettings settings)
        {
            _configurations.DefineConfiguration(type, settings);
            return this;
        }

        public IComparer<T> GetComparer<T>() => (IComparer<T>)GetComparer(typeof(T));

        public IComparer GetComparer(Type objectType)
        {
            if (objectType.GetUnderlyingType().IsPrimitive())
            {
                throw new NotSupportedException(
                    $"Generation a comparer for primitive type {objectType.FullName} is not supported.");
            }

            return _comparers.GetOrAdd(
                objectType,
                key => _context.GetOrBuildComparerType(key).CreateInstance<IComparerContext, IComparer>(_context));
        }

        public IEqualityComparer<T> GetEqualityComparer<T>() => throw new NotImplementedException();

        public IEqualityComparer GetEqualityComparer(Type objectType) => throw new NotImplementedException();

        public IContextBuilder<T> For<T>() => new GenericProxy<T>(this);

        private sealed class GenericProxy<T> : IContextBuilder<T>, IComparerProviderOrBuilderContext<T>
        {
            private readonly ComparersBuilder _owner;

            public GenericProxy(ComparersBuilder comparersBuilder) => _owner = comparersBuilder;

            public IContextBuilder<TOther> For<TOther>() => _owner.For<TOther>();

            public IComparerProviderOrBuilderContext<T> DefineConfiguration(ComparerSettings settings)
            {
                _owner.DefineConfiguration(typeof(T), settings);
                return this;
            }

            public IComparer<T> GetComparer() => _owner.GetComparer<T>();

            public IEqualityComparer<T> GetEqualityComparer() => _owner.GetEqualityComparer<T>();
        }
    }
}
