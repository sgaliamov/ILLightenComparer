using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ILLightenComparer
{
    /// <summary>
    ///     Interface provides methods to build an instance of a comparer based on defined type and configuration.
    /// </summary>
    public interface IComparerBuilder : IComparerProvider, IEqualityComparerProvider
    {
        /// <summary>
        ///     Defines configuration for generated comparers.
        ///     Creates another builder context.
        /// </summary>
        /// <param name="config">Callback action to configure comparers.</param>
        /// <returns>Self.</returns>
        IComparerBuilder Configure(Action<IConfigurationBuilder> config);

        /// <summary>
        ///     Converts the common builder to typed version.
        ///     Defines type for the following methods.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <returns>Typed instance of comparer builder.</returns>
        IComparerBuilder<T> For<T>();

        /// <summary>
        ///     Defines configuration for comparer of type <see cref="IComparer{T}" />.
        ///     Defines type for the following methods.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <param name="config">Callback action to configure comparer for defined type <typeparamref name="T" />.</param>
        /// <returns>Typed instance of comparer builder.</returns>
        IComparerBuilder<T> For<T>(Action<IConfigurationBuilder<T>> config);
    }

    /// <summary>
    ///     Interface to build an instance of a comparer based for provided type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type whose instances need to compare.</typeparam>
    public interface IComparerBuilder<T> : IComparerProvider<T>, IEqualityComparerProvider<T>
    {
        /// <summary>
        ///     Self.
        /// </summary>
        IComparerBuilder Builder { get; }

        /// <summary>
        ///     Defines configuration for comparer of type <see cref="IComparer{T}" />.
        ///     Creates another builder context.
        /// </summary>
        /// <param name="config">Callback action to configure comparer for defined type <typeparamref name="T" />.</param>
        /// <returns>Typed instance of comparer builder.</returns>
        IComparerBuilder<T> Configure(Action<IConfigurationBuilder<T>> config);

        /// <summary>
        ///     Defines next type for the following methods.
        /// </summary>
        /// <typeparam name="TOther">The type whose instances need to compare.</typeparam>
        /// <returns>Typed instance of comparer builder.</returns>
        IComparerBuilder<TOther> For<TOther>();

        /// <summary>
        ///     Defines configuration for comparer of type <see cref="IComparer{T}" />.
        ///     Defines next type for the following methods.
        /// </summary>
        /// <typeparam name="TOther">The type whose instances need to compare.</typeparam>
        /// <param name="config">Callback action to configure comparer for defined type <typeparamref name="TOther" />.</param>
        /// <returns>Typed instance of comparer builder.</returns>
        IComparerBuilder<TOther> For<TOther>(Action<IConfigurationBuilder<TOther>> config);
    }

    /// <summary>
    ///     Provides methods to define default configuration for generated comparers.
    /// </summary>
    public interface IDefaultConfigurationBuilder
    {
        /// <summary>
        ///     If enabled collections order is ignored during comparison by default.
        /// </summary>
        /// <param name="value">
        ///     Default value false.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetDefaultCollectionsOrderIgnoring(bool? value);

        /// <summary>
        ///     Enables or disables default cycle detection.
        /// </summary>
        /// <param name="value">
        ///     Default value true.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetDefaultCyclesDetection(bool? value);

        /// <summary>
        ///     If enabled fields are used to generated comparer by default.
        /// </summary>
        /// <param name="value">
        ///     Default value is true.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetDefaultFieldsInclusion(bool? value);

        /// <summary>
        ///     Defines default hash seed.
        /// </summary>
        /// <param name="value">
        ///     Default value is 0x1505L.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetDefaultHashSeed(long? value);

        /// <summary>
        ///     Defines default <see cref="StringComparison" /> type.
        /// </summary>
        /// <param name="value">
        ///     Default value is <see cref="StringComparison.Ordinal" />.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetDefaultStringComparisonType(StringComparison? value);
    }

    /// <summary>
    ///     Helper interface to define members order.
    /// </summary>
    /// <typeparam name="T">Type of a comparable object.</typeparam>
    public interface IMembersOrder<T>
    {
        /// <summary>
        ///     Defines order to compare of a member.
        /// </summary>
        /// <typeparam name="TMember">Type of a member.</typeparam>
        /// <param name="selector">Selector to define a member.</param>
        /// <returns>Members order helper.</returns>
        IMembersOrder<T> Member<TMember>(Expression<Func<T, TMember>> selector);
    }

    /// <summary>
    ///     Provides methods to configure specific comparer.
    /// </summary>
    public interface IConfigurationBuilder : IDefaultConfigurationBuilder
    {
        /// <summary>
        ///     Defines configuration for comparer of type <see cref="IComparer{T}" />.
        ///     Creates another builder context.
        ///     Defines next type for the following methods.
        /// </summary>
        /// <param name="config">Callback action to configure comparer for defined type <typeparamref name="T" />.</param>
        /// <returns>Typed instance of comparer builder.</returns>
        IConfigurationBuilder<T> ConfigureFor<T>(Action<IConfigurationBuilder<T>> config);

        /// <summary>
        ///     Defines order in which members should be compared.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <param name="order">Callback to configure order of members.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder DefineMembersOrder<T>(Action<IMembersOrder<T>> order);

        /// <summary>
        ///     Enables or disables cycle detection.
        /// </summary>
        /// <param name="type">The type whose instances need to compare.</param>
        /// <param name="value">
        ///     Default value true.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder DetectCycles(Type type, bool? value);

        /// <summary>
        ///     If enabled collections order is ignored during comparison.
        /// </summary>
        /// <param name="type">The type whose instances need to compare.</param>
        /// <param name="value">
        ///     Default value false.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder IgnoreCollectionsOrder(Type type, bool? value);

        /// <summary>
        ///     Defines list of members to ignore.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <typeparam name="TMember">Type of a member.</typeparam>
        /// <param name="selectors">Selectors to define list of members.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder IgnoreMember<T, TMember>(params Expression<Func<T, TMember>>[] selectors);

        /// <summary>
        ///     If enabled fields are used to generated comparer.
        /// </summary>
        /// <param name="type">The type whose instances need to compare.</param>
        /// <param name="value">
        ///     Default value is true.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder IncludeFields(Type type, bool? value);

        /// <summary>
        ///     Defines custom comparer.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <param name="instance">Instance of a comparer.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetCustomComparer<T>(IComparer<T> instance);

        /// <summary>
        ///     Defines type of custom comparer.
        ///     Type must has default constructor.
        /// </summary>
        /// <typeparam name="TComparer">Type of custom comparer.</typeparam>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetCustomComparer<TComparer>();

        /// <summary>
        ///     Defines custom equality comparer.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <param name="instance">Instance of a comparer.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetCustomEqualityComparer<T>(IEqualityComparer<T> instance);

        /// <summary>
        ///     Defines type of custom equality comparer.
        ///     Type must has default constructor.
        /// </summary>
        /// <typeparam name="TComparer">Type of custom comparer.</typeparam>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetCustomEqualityComparer<TComparer>();

        /// <summary>
        ///     Defines hash seed for the <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type whose instances need to be compared.</param>
        /// <param name="value">
        ///     Default value is 0x1505L.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetHashSeed(Type type, long? value);

        /// <summary>
        ///     Defines <see cref="StringComparison" /> type for strings comparison.
        /// </summary>
        /// <param name="type">The type whose instances need to compare.</param>
        /// <param name="value">
        ///     Default value is <see cref="StringComparison.Ordinal" />.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder SetStringComparisonType(Type type, StringComparison? value);
    }

    /// <summary>
    ///     Provides typed methods to configure specific comparer.
    /// </summary>
    public interface IConfigurationBuilder<T>
    {
        /// <summary>
        ///     Defines order in which members should be compared.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <param name="order">Callback to configure order of members.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder<T> DefineMembersOrder(Action<IMembersOrder<T>> order);

        /// <summary>
        ///     Enables or disables cycle detection.
        /// </summary>
        /// <param name="value">
        ///     Default value true.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder<T> DetectCycles(bool? value);

        /// <summary>
        ///     If enabled collections order is ignored during comparison.
        /// </summary>
        /// <param name="value">
        ///     Default value false.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder<T> IgnoreCollectionsOrder(bool? value);

        /// <summary>
        ///     Defines list of members to ignore.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <typeparam name="TMember">Type of a member.</typeparam>
        /// <param name="selectors">Selectors to define list of members.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder<T> IgnoreMember<TMember>(params Expression<Func<T, TMember>>[] selectors);

        /// <summary>
        ///     If enabled fields are used to generated comparer.
        /// </summary>
        /// <param name="value">
        ///     Default value is true.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder<T> IncludeFields(bool? value);

        /// <summary>
        ///     Defined <see cref="StringComparison" /> type for strings comparison.
        /// </summary>
        /// <param name="value">
        ///     Default value is <see cref="StringComparison.Ordinal" />.
        ///     null value resets to default.
        /// </param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder<T> SetStringComparisonType(StringComparison? value);
    }

    /// <summary>
    ///     Provides access to generated comparers.
    /// </summary>
    public interface IComparerProvider
    {
        /// <summary>
        ///     Returns an instance of comparer.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <returns>Instance of <see cref="IComparer{T}" />.</returns>
        IComparer<T> GetComparer<T>();
    }

    /// <summary>
    ///     Provides access to generated comparers.
    /// </summary>
    public interface IEqualityComparerProvider
    {
        /// <summary>
        ///     Returns an instance of equality comparer.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <returns>Instance of <see cref="IEqualityComparer{T}" />.</returns>
        IEqualityComparer<T> GetEqualityComparer<T>();
    }

    /// <summary>
    ///     Provides access to generated comparers.
    /// </summary>
    public interface IComparerProvider<in T>
    {
        /// <summary>
        ///     Returns an instance of comparer.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <returns>Instance of <see cref="IComparer{T}" />.</returns>
        IComparer<T> GetComparer();

        /// <summary>
        ///     Returns an instance of comparer.
        /// </summary>
        /// <typeparam name="TOther">The type whose instances need to compare.</typeparam>
        /// <returns>Instance of <see cref="IComparer{TOther}" />.</returns>
        IComparer<TOther> GetComparer<TOther>();
    }

    /// <summary>
    ///     Provides access to generated equality comparers.
    /// </summary>
    public interface IEqualityComparerProvider<in T>
    {
        /// <summary>
        ///     Returns an instance of equality comparer.
        /// </summary>
        /// <typeparam name="T">The type whose instances need to compare.</typeparam>
        /// <returns>Instance of <see cref="IEqualityComparer{T}" />.</returns>
        IEqualityComparer<T> GetEqualityComparer();

        /// <summary>
        ///     Returns an instance of equality comparer.
        /// </summary>
        /// <typeparam name="TOther">The type whose instances need to compare.</typeparam>
        /// <returns>Instance of <see cref="IEqualityComparer{TOther}" />.</returns>
        IEqualityComparer<TOther> GetEqualityComparer<TOther>();
    }
}
