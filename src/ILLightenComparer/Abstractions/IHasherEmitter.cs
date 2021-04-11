using System.Reflection.Emit;
using Illuminator;
using static ILLightenComparer.Extensions.Functions;
using static Illuminator.Functions;

namespace ILLightenComparer.Abstractions
{
    internal interface IHasherEmitter
    {
        /// <summary>
        ///     Hashing logic builder.
        /// </summary>
        ILEmitter Emit(ILEmitter il);

        /// <summary>
        ///     Hashing logic builder with variable to accumulate hash.
        /// </summary>
        ILEmitter Emit(ILEmitter il, LocalBuilder hash);
    }

    internal static class HasherEmitterExtensions
    {
        public static ILEmitterFunc Emit(this IHasherEmitter hasher, LocalBuilder hash) =>
            (in ILEmitter il) => hasher.Emit(il, hash);

        public static ILEmitter EmitHashing(this IHasherEmitter hasher, ILEmitter il, LocalBuilder hash)
        {
            var add = Add(
                Shl(Ldloc(hash), Ldc_I4(5)),
                Ldloc(hash));

            return il
                   .Xor(add, Cast<long>(hasher.Emit(hash))) // todo: 2. need to cast?
                   .Stloc(hash);
        }
    }
}
