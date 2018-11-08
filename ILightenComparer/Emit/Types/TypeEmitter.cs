﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using ILightenComparer.Emit.Extensions;
using ILightenComparer.Reflection;

namespace ILightenComparer.Emit.Types
{
    internal sealed class TypeEmitter
    {
        private readonly ModuleBuilder _module;

        public TypeEmitter()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("ILLightenComparer.DynamicAssembly"),
                AssemblyBuilderAccess.Run);

            _module = assembly.DefineDynamicModule("ILLightenComparer.Module");
        }

        public Func<TReturnType> EmitFactoryMethod<TReturnType>(TypeBuilder type)
        {
            var method = type.DefineMethod(
                "GetInstance",
                MethodAttributes.Static,
                type,
                null);

            var il = method.GetILGenerator();
            il.Emit(OpCodes.Newobj, type.GetConstructor(null));
            il.Emit(OpCodes.Ret);

            return method.CreateDelegate<Func<TReturnType>>();
        }

        public TypeBuilder DefineType(string name)
        {
            var type = _module.DefineType(name);
            type.AddInterfaceImplementation(Interface.Comparer);
            type.AddInterfaceImplementation(Interface.GenericComparer);

            return type;
        }
    }
}