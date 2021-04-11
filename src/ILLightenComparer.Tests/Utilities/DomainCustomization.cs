using System.Globalization;
using System.Linq;
using AutoFixture;

namespace ILLightenComparer.Tests.Utilities
{
    internal sealed class DomainCustomization : ICustomization
    {
        private static void AddGenerators(IFixture fixture)
        {
            fixture.Customizations.Add(
                new StringGenerator(() => (fixture.Create<ushort>() % 4).ToString(CultureInfo.InvariantCulture)));

            fixture.Customizations.Insert(0, new CasualNullGenerator(Constants.NullProbability));
            fixture.Customizations.Insert(0, new CustomNumericGenerator(-1, 1, 0.1));
            var enumGenerator = fixture.Customizations.First(x => x is EnumGenerator);
            fixture.Customizations.Remove(enumGenerator);
        }

        public void Customize(IFixture fixture)
        {
            AddGenerators(fixture);

            fixture.Customize(new SupportMutableValueTypesCustomization());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
    }
}
