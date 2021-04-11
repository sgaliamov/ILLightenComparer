using System;
using System.Collections;
using System.Threading.Tasks;
using ILLightenComparer.Tests.Comparers;
using ILLightenComparer.Tests.Samples;
using ILLightenComparer.Tests.Utilities;
using Xunit;

namespace ILLightenComparer.Tests.ComparerTests
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
                    var comparer = (IComparer)Activator.CreateInstance(comparerType, referenceComparer);

                    new GenericTests().GenericTest(objectType, comparer, false, Constants.SmallCount);
                });
        }

        [Fact]
        public void Compare_sample_objects()
        {
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), false);
            Test(typeof(SampleObject<>), typeof(SampleObjectComparer<>), true);
        }

        [Fact]
        public void Compare_sample_structs()
        {
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), false);
            Test(typeof(SampleStruct<>), typeof(SampleStructComparer<>), true);
        }
    }
}
