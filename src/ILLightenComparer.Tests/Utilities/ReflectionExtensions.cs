using System;

namespace ILLightenComparer.Tests.Utilities
{
    internal static class ReflectionExtensions
    {
        public static Type MakeNullable(this Type type)
        {
            if (!type.IsValueType) {
                throw new ArgumentException(nameof(type));
            }

            return typeof(Nullable<>).MakeGenericType(type);
        }

        public static bool IsSealedType(this Type type) => type.IsValueType || type.IsSealed;

        public static bool IsNullable(this Type type) =>
            type.IsValueType
            && type.IsGenericType
            && !type.IsGenericTypeDefinition
            && ReferenceEquals(type.GetGenericTypeDefinition(), typeof(Nullable<>));

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
                throw new ArgumentException($"{generic.FullName} should be generic type.", nameof(generic));
            }

            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == generic) {
                return type;
            }

            return Array.Find(
                type.GetInterfaces(),
                t => t.IsGenericType && generic == t.GetGenericTypeDefinition());
        }
    }
}
