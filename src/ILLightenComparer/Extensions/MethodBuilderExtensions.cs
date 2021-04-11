using System.Reflection.Emit;
using Illuminator;

namespace ILLightenComparer.Extensions
{
    internal static class MethodBuilderExtensions
    {
        public static ILEmitter CreateILEmitter(this MethodBuilder methodBuilder)
        {
            #if DEBUG
            return methodBuilder.GetILGenerator().UseIlluminator(true);
            #else
            return methodBuilder.GetILGenerator().UseIlluminator();
            #endif
        }

        public static ILEmitter CreateILEmitter(this ConstructorBuilder constructorBuilder)
        {
            #if DEBUG
            return constructorBuilder.GetILGenerator().UseIlluminator(true);
            #else
            return constructorBuilder.GetILGenerator().UseIlluminator();
            #endif
        }
    }
}
