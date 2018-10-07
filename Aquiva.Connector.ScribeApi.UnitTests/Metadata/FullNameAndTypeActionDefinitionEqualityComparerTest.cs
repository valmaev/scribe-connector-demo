using AutoFixture.Xunit2;
using Scribe.Core.ConnectorApi.Metadata;
using Xunit;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class FullNameAndTypeActionDefinitionEqualityComparerTest
    {
        [Fact]
        public void FullNameAndTypeActionDefinitionEqualityComparer_Equals_WithNullArguments_ShouldReturnFalse()
        {
            var sut = new FullNameAndTypeActionDefinitionEqualityComparer();
            Assert.False(sut.Equals(null, new ActionDefinition()));
            Assert.False(sut.Equals(new ActionDefinition(), null));
        }

        [Fact]
        public void FullNameAndTypeActionDefinitionEqualityComparer_Equals_WithSameObject_ShouldReturnTrue()
        {
            var input = new ActionDefinition();
            var sut = new FullNameAndTypeActionDefinitionEqualityComparer();
            Assert.True(sut.Equals(input, input));
            Assert.Equal(sut.GetHashCode(input), sut.GetHashCode(input));
        }

        [Fact]
        public void FullNameAndTypeActionDefinitionEqualityComparer_Equals_WithDifferentSubTypes_ShouldReturnFalse()
        {
            var x = new DummyActionDefinition();
            var y = new ActionDefinition();
            var sut = new FullNameAndTypeActionDefinitionEqualityComparer();

            Assert.False(sut.Equals(x, y));
            Assert.False(sut.Equals(y, x));
        }

        [Theory]
        [AutoData]
        public void FullNameAndTypeActionDefinitionEqualityComparer_Equals_WithDifferentFullNameAndDescriptionCombination_ShouldReturnFalse(
            string xFullName,
            string xDescription,
            string yFullName,
            string yDescription,
            ActionDefinition x,
            ActionDefinition y)
        {
            x.FullName = xFullName;
            x.Description = xDescription;
            y.FullName = yFullName;
            y.Description = yDescription;
            var sut = new FullNameAndTypeActionDefinitionEqualityComparer();

            Assert.False(sut.Equals(x, y));
            Assert.NotEqual(sut.GetHashCode(x), sut.GetHashCode(y));
        }

        [Theory]
        [AutoData]
        public void FullNameAndTypeActionDefinitionEqualityComparer_Equals_WithPairlyEqualFullNameAndDescription_ShouldReturnTrue(
            string fullName,
            string description,
            ActionDefinition x,
            ActionDefinition y)
        {
            x.FullName = fullName;
            x.Description = description;
            y.FullName = fullName;
            y.Description = description;
            var sut = new FullNameAndTypeActionDefinitionEqualityComparer();

            Assert.True(sut.Equals(x, y));
            Assert.Equal(sut.GetHashCode(x), sut.GetHashCode(y));
        }

        private class DummyActionDefinition : ActionDefinition
        {
        }
    }
}
