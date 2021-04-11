using System;
using System.Collections;
using System.Threading.Tasks;
using ILLightenComparer.Tests.EqualityComparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.EqualityTests
{
    public sealed class SampleMembersTests
    {
        private static void Test(Type genericSampleType, Type genericSampleComparer, bool nullable)
        {
            var types = nullable ? TestTypes.NullableTypes : TestTypes.Types;

            Parallel.ForEach(
                types,
                item => {
                    var (type, referenceComparer) = item;
                    var objectType = genericSampleType.MakeGenericType(type);
                    var comparerType = genericSampleComparer.MakeGenericType(type);
                    var comparer = (IEqualityComparer)Activator.CreateInstance(comparerType, referenceComparer);

                    new GenericTests(false).GenericTest(objectType, comparer, Constants.SmallCount);
                });
        }

        [Fact]
        public void Compare_sample_objects()
        {
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), false);
            Test(typeof(ComparableObject<>), typeof(ComparableObjectEqualityComparer<>), true);
        }

        [Fact]
        public void Compare_sample_structs()
        {
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), false);
            Test(typeof(ComparableStruct<>), typeof(ComparableStructEqualityComparer<>), true);
        }
    }
}
