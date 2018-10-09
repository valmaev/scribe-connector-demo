using System;
using System.ComponentModel;
using Scribe.Core.ConnectorApi.Metadata;
using Xunit;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class ActionDefinitionAttributeTest
    {
        [Fact]
        public void ActionDefinitionAttribute_Constructor_WithNonDefinedKnownActionType_ShouldThrow()
        {
            var nonDefinedKnownAction = int.MaxValue;
            Assert.False(Enum.IsDefined(typeof(KnownActions), nonDefinedKnownAction));

            var actual = Assert.Throws<InvalidEnumArgumentException>(
                () => new ActionDefinitionAttribute((KnownActions) nonDefinedKnownAction));
            Assert.Contains(nonDefinedKnownAction.ToString(), actual.Message);
            Assert.Contains(typeof(KnownActions).Name, actual.Message);
        }

        [Theory]
        [InlineData(KnownActions.Create)]
        [InlineData(KnownActions.CreateWith)]
        [InlineData(KnownActions.Delete)]
        [InlineData(KnownActions.InsertUpdate)]
        [InlineData(KnownActions.NativeQuery)]
        [InlineData(KnownActions.None)]
        [InlineData(KnownActions.Query)]
        [InlineData(KnownActions.Update)]
        [InlineData(KnownActions.UpdateInsert)]
        [InlineData(KnownActions.UpdateWith)]
        public void ActionDefinitionAttribute_Constructor_WithValidKnownActionType_ShouldInitProperties(
            KnownActions expected)
        {
            var sut = new ActionDefinitionAttribute(expected);
            Assert.Equal(expected, sut.KnownActionType);
            Assert.Equal(expected.ToString(), sut.Name);
            Assert.Equal(expected.ToString(), sut.FullName);
            Assert.Equal(expected.ToString(), sut.Description);
        }
    }
}
