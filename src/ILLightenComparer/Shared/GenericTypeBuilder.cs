using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ILLightenComparer.Abstractions;
using ILLightenComparer.Config;
using ILLightenComparer.Extensions;
using ILLightenComparer.Variables;
using Illuminator;
using static ILLightenComparer.Extensions.Functions;
using TypeExtensions = ILLightenComparer.Extensions.TypeExtensions;

namespace ILLightenComparer.Shared
{
    internal sealed class GenericTypeBuilder
    {
        private static readonly Type ContextType = typeof(IContext);

        private static void BuildConstructorAndFactoryMethod(TypeBuilder typeBuilder, FieldInfo contextField)
        {
            var parameters = new[] { typeof(IContext) };

            var constructorInfo = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                parameters);

            using (var il = constructorInfo.CreateILEmitter()) {
                il.LoadArgument(Arg.This)
                  .Call(typeof(object).GetConstructor(Type.EmptyTypes))
                  .Stfld(LoadArgument(Arg.This), LoadArgument(1), contextField)
                  .Ret();
            }

            var methodBuilder = typeBuilder.DefineStaticMethod(
                nameof(TypeExtensions.CreateInstance),
                typeBuilder,
                parameters);

            using (var il = methodBuilder.CreateILEmitter()) {
                il.Newobj(constructorInfo, LoadArgument(Arg.This)).Ret();
            }
        }

        private readonly IConfigurationProvider _configuration;
        private readonly IReadOnlyDictionary<string, IStaticMethodEmitter> _methodEmitter;

        public GenericTypeBuilder(
            IReadOnlyDictionary<string, IStaticMethodEmitter> methodEmitters,
            IConfigurationProvider configuration)
        {
            _methodEmitter = methodEmitters;
            _configuration = configuration;
        }

        public Type Build(StaticMethodsInfo methodsInfo)
        {
            var comparerTypeBuilder = methodsInfo.ComparerTypeBuilder;
            var objectType = methodsInfo.ObjectType;
            var contextField = comparerTypeBuilder.DefineField(
                "_context",
                ContextType,
                FieldAttributes.InitOnly | FieldAttributes.Private);

            BuildConstructorAndFactoryMethod(comparerTypeBuilder, contextField);

            var names = methodsInfo
                        .GetAllMethodBuilders()
                        .Select(staticMethodBuilder => {
                            BuildStaticMethod(objectType, staticMethodBuilder);
                            BuildInstanceMethod(
                                methodsInfo.TypedComparerInterface,
                                comparerTypeBuilder,
                                staticMethodBuilder,
                                contextField,
                                objectType);

                            return staticMethodBuilder.Name;
                        })
                        .ToArray();

            var compiledComparerType = comparerTypeBuilder.CreateTypeInfo();

            foreach (var name in names) {
                var method = compiledComparerType!.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
                methodsInfo.SetCompiledMethod(method);
            }

            return compiledComparerType;
        }

        private void BuildInstanceMethod(
            Type typedComparerInterface,
            TypeBuilder typeBuilder,
            MethodBuilder staticMethod,
            FieldInfo contextField,
            Type objectType)
        {
            var interfaceMethod = typedComparerInterface.GetMethod(staticMethod.Name);
            var parametersCount = interfaceMethod!.GetParameters().Length;
            var methodBuilder = typeBuilder.DefineInterfaceMethod(interfaceMethod);

            using var il = methodBuilder.CreateILEmitter();

            Enumerable.Range(0, parametersCount)
                      .Aggregate(
                          il.LoadArgument(Arg.This).Ldfld(contextField),
                          (emitter, index) => emitter.LoadArgument(index + 1));

            EmitStaticMethodCall(il, objectType, staticMethod, parametersCount);
        }

        private void BuildStaticMethod(Type objectType, MethodBuilder staticMethodBuilder)
        {
            var detectCycles = NeedDetectCycles(objectType, staticMethodBuilder.Name);
            _methodEmitter[staticMethodBuilder.Name].Build(objectType, detectCycles, staticMethodBuilder);
        }

        private void EmitStaticMethodCall(ILEmitter il, Type objectType, MethodBuilder staticMethod, int parametersCount)
        {
            var createCycleDetectionSets = NeedCreateCycleDetectionSets(objectType, staticMethod.Name);

            Enumerable.Range(0, parametersCount)
                      .Aggregate(il, (emitter, _) => createCycleDetectionSets
                                     ? emitter.Newobj(CycleDetectionSet.DefaultConstructor)
                                     : emitter.Ldnull())
                      .CallMethod(staticMethod)
                      .Ret();
        }

        private bool NeedCreateCycleDetectionSets(Type objectType, string methodName) =>
            _configuration.Get(objectType).DetectCycles
            && !objectType.IsPrimitive()
            && _methodEmitter[methodName].NeedCreateCycleDetectionSets(objectType.GetUnderlyingType());

        private bool NeedDetectCycles(Type objectType, string methodName) =>
            objectType != typeof(object) // indirect comparison will be triggered
            && NeedCreateCycleDetectionSets(objectType, methodName)
            && !objectType.IsNullable()
            && !objectType.ImplementsGenericInterface(typeof(IEnumerable<>));
    }
}
