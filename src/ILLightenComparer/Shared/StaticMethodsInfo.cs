using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ILLightenComparer.Shared
{
    internal sealed class StaticMethodsInfo
    {
        private readonly IDictionary<string, (MethodInfo, bool)> _methods;

        public StaticMethodsInfo(
            Type objectType,
            Type typedComparerInterface,
            TypeBuilder comparerTypeBuilder,
            IReadOnlyCollection<MethodBuilder> methods)
        {
            ObjectType = objectType;
            ComparerTypeBuilder = comparerTypeBuilder;
            TypedComparerInterface = typedComparerInterface;
            _methods = methods.ToDictionary(x => x.Name, x => ((MethodInfo)x, false));
        }

        public Type ObjectType { get; }
        public Type TypedComparerInterface { get; }
        public TypeBuilder ComparerTypeBuilder { get; }

        public bool AllCompiled() => _methods.All(x => x.Value.Item2);

        public IReadOnlyCollection<MethodBuilder> GetAllMethodBuilders() =>
            _methods
                .Values
                .Select(x => (MethodBuilder)x.Item1)
                .ToArray();

        public MethodInfo GetMethodInfo(string name) => _methods[name].Item1;

        public bool IsCompiled(string name) => _methods.TryGetValue(name, out var info) && info.Item2;

        public void SetCompiledMethod(MethodInfo method) => _methods[method.Name] = (method, true);
    }
}
