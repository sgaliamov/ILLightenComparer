﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Reflection;

namespace ILLightenComparer.Emit.Extensions
{
    internal static class TypeBuilderExtensions
    {
        public static MethodBuilder DefineInterfaceMethod(this TypeBuilder typeBuilder, MethodInfo interfaceMethod)
        {
            var method = typeBuilder.DefineMethod(
                interfaceMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                interfaceMethod.ReturnType,
                interfaceMethod.GetParameters().Select(x => x.ParameterType).ToArray()
            );

            typeBuilder.DefineMethodOverride(method, interfaceMethod);

            return method;
        }

        public static MethodBuilder DefineStaticMethod(
            this TypeBuilder staticTypeBuilder,
            string name,
            Type returnType,
            Type[] parameterTypes) =>
            staticTypeBuilder.DefineMethod(
                name,
                MethodAttributes.Public | MethodAttributes.Static,
                CallingConventions.Standard,
                returnType,
                parameterTypes);

        public static void BuildFactoryMethod<TReturnType>(this TypeBuilder typeBuilder)
        {
            var constructorInfo = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            var method = DefineStaticMethod(
                typeBuilder,
                Constants.FactoryMethodName,
                typeof(TReturnType),
                null);

            EmitCallCtor(method.GetILGenerator(), constructorInfo);
        }

        private static void EmitCallCtor(ILGenerator ilGenerator, ConstructorInfo constructor)
        {
            ilGenerator.Emit(OpCodes.Newobj, constructor);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}