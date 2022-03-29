using FluentAssertions;
using NUnit.Framework;
using TypeParser;
using TypeParser.UtilityClasses;

namespace UnitTests
{
    [TestFixture]
    public class AlternativeTests
    {
        [Test]
        public void AsObjectTest()
        {
            var alternative = TypeCompiler.Parse<IAlternative<int, string>>("1");
            alternative.AsObject().Should().BeOfType<int>();
        }
    }
}