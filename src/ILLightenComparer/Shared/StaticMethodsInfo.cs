﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ILLightenComparer.Shared
{
    internal sealed class StaticMethodsInfo
    {
        private readonly IDictionary<string, (MethodInfo, bool)> _methods;

        public StaticMethodsInfo(IReadOnlyCollection<MethodBuilder> methods) =>
            _methods = methods.ToDictionary(x => x.Name, x => ((MethodInfo)x, false));

        public bool IsCompiled(string name) =>
            _methods.TryGetValue(name, out var info) && info.Item2;

        public bool AllCompiled() => _methods.All(x => x.Value.Item2);

        public MethodInfo GetMethodInfo(string name) => _methods[name].Item1;

        public void SetCompiledMethod(MethodInfo method) => _methods[method.Name] = (method, true);
    }
}