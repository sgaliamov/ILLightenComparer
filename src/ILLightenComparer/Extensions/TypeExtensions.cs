using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace ILLightenComparer.Extensions
{
    internal static class TypeExtensions
    {
        private static readonly HashSet<Type> SmallIntegralTypes = new(new[] {
            typeof(sbyte),
            typeof(byte),
            typeof(char),
            typeof(short),
            typeof(ushort)
        });

        private static readonly HashSet<Type> CeqCompatibleTypes = new(new[] {
            typeof(sbyte),
            typeof(byte),
            typeof(char),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double)
        });

        private static readonly HashSet<Type> BasicTypes =
            new(typeof(object).Assembly
                              .GetTypes()
                              .Where(x => x.FullName!.StartsWith("System."))
                              .Where(x => x.IsPublic)
                              .Where(x => !x.IsGenericType)
                              .Except(new[] { typeof(object) })); // object is treated separately

        /// <summary>
        ///     Not objects and structures.
        ///     Extended version of IsPrimitive property.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if the <paramref name="type">type</paramref> is one of the primitive types; otherwise, false.</returns>
        public static bool IsPrimitive(this Type type) =>
            type.IsPrimitive
            || type.IsEnum
            || ReferenceEquals(type, typeof(string))
            // || ReferenceEquals(type, typeof(object)) // todo: 3. move extensions to separate assembly
            || ReferenceEquals(type, typeof(decimal));

        public static bool ImplementsGenericInterface(this Type type, Type generic) => type.FindGenericInterface(generic) != null;

        public static Type FindGenericInterface(this Type type, Type generic)
        {
            if (!generic.IsGenericType) {
                throw new ArgumentException($"{generic.DisplayName()} should be generic type.", nameof(generic));
            }

            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == generic) {
                return type;
            }

            return Array.Find(
                type.GetInterfaces(),
                t => t.IsGenericType && generic == t.GetGenericTypeDefinition());
        }

        public static bool IsNullable(this Type type) =>
            type.IsValueType
            && type.IsGenericType
            && !type.IsGenericTypeDefinition
            && ReferenceEquals(type.GetGenericTypeDefinition(), typeof(Nullable<>));

        /// <summary>
        ///     Returns the underlying type of the specified Enum or Nullable.
        /// </summary>
        /// <param name="type">The type whose underlying type will be retrieved.</param>
        /// <returns>The underlying type of <paramref name="type">type</paramref>.</returns>
        public static Type GetUnderlyingType(this Type type)
        {
            while (true) {
                if (type.IsEnum) {
                    return Enum.GetUnderlyingType(type);
                }

                if (type.IsNullable()) {
                    type = type.GetGenericArguments()[0];
                } else {
                    return type;
                }
            }
        }

        /// <summary>
        ///     Creates instance using static method.
        /// </summary>
        public static TReturnType CreateInstance<T, TReturnType>(this Type type, T arg) =>
            type.GetMethod(nameof(CreateInstance))
                .CreateDelegate<Func<T, TReturnType>>()(arg); // todo: 1. cache delegates?

        /// <summary>
        ///     Creates instance using default constructor.
        /// </summary>
        public static TResult Create<TResult>(this Type type)
        {
            // todo: 2. benchmark creation
            var ctor = type.GetConstructor(Type.EmptyTypes)
                       ?? throw new ArgumentException($"Type {type.DisplayName()} should has default constructor.", nameof(type));

            var lambda = Expression.Lambda(typeof(Func<TResult>), Expression.New(ctor));
            var compiled = (Func<TResult>)lambda.Compile();

            return compiled();
        }

        public static MethodInfo GetUnderlyingCompareToMethod(this Type type)
        {
            var underlyingType = type.GetUnderlyingType();

            return underlyingType.FindMethod(nameof(IComparable.CompareTo), new[] { underlyingType });
        }

        public static MethodInfo GetPropertyGetter(this Type type, string name) =>
            type.GetProperty(name)?.GetGetMethod()
            ?? throw new ArgumentException($"{type.DisplayName()} does not have {name} property.");

        public static bool IsIntegral(this Type type) => SmallIntegralTypes.Contains(type);

        public static bool IsCeqCompatible(this Type type) => CeqCompatibleTypes.Contains(type);

        public static bool IsBasic(this Type type) => BasicTypes.Contains(type);

        public static bool IsSealedType(this Type type) => type.IsValueType || type.IsSealed;

        public static bool IsComparable(this Type type) => type.ImplementsGenericInterface(typeof(IComparable<>));

        public static bool IsHierarchical(this Type type) =>
            !type.IsPrimitive()
            && !type.IsNullable()
            && !typeof(IEnumerable).IsAssignableFrom(type);

        public static MethodInfo FindMethod(this Type type, string name, Type[] types)
        {
            if (type.IsInterface) {
                return type.GetMethod(name, types)
                       ?? type.GetInterfaces()
                              .Select(i => FindMethod(i, name, types))
                              .FirstOrDefault(m => m != null);
            }

            return type.GetMethod(name, types);
        }

        public static MethodBuilder DefineStaticMethod(
            this TypeBuilder staticTypeBuilder,
            string name,
            Type returnType,
            Type[] parameterTypes)
        {
            var methodBuilder = staticTypeBuilder.DefineMethod(
                name,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.Final,
                CallingConventions.Standard,
                returnType,
                parameterTypes);

            methodBuilder.SetImplementationFlags(MethodImplAttributes.AggressiveInlining | MethodImplAttributes.IL);

            return methodBuilder;
        }
    }
}
