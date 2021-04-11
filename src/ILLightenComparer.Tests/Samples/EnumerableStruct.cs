using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ILLightenComparer.Tests.Samples
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Test class")]
    public struct EnumerableStruct<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _enumerable;

        public EnumerableStruct(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
        }

        public IEnumerator<T> GetEnumerator() => _enumerable?.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
