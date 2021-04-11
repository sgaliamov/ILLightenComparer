using System;
using System.Reflection.Emit;

namespace ILLightenComparer.Abstractions
{
    internal interface IStaticMethodEmitter
    {
        void Build(Type objectType, bool detectCycles, MethodBuilder staticMethodBuilder);
        bool NeedCreateCycleDetectionSets(Type objectType);
    }
}
