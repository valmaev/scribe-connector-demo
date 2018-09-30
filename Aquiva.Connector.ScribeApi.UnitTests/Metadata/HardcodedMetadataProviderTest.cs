using System;
using System.Collections.Generic;
using System.Linq;
using Scribe.Core.ConnectorApi.Metadata;
using Xunit;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class HardcodedMetadataProviderTest
    {
        [Fact]
        public void HardcodedMetadataProvider_RetrieveActionDefinitions_Always_ShouldReturnCorrectMetadataForQuery()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveActionDefinitions()
                .Single(x => x.FullName == nameof(KnownActions.Query));
            
            Assert.Equal(KnownActions.Query, actual.KnownActionType);
            Assert.Equal(nameof(KnownActions.Query), actual.Name);
            Assert.False(actual.SupportsBulk);
            Assert.True(actual.SupportsConstraints);
            Assert.False(actual.SupportsInput);
            Assert.True(actual.SupportsLookupConditions);
            Assert.False(actual.SupportsMultipleRecordOperations);
            Assert.False(actual.SupportsRelations);
            Assert.True(actual.SupportsSequences);
        }

        [Fact]
        public void HardcodedMetadataProvider_RetrieveActionDefinitions_Always_ShouldReturnCorrectMetadataForCreate()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveActionDefinitions()
                .Single(x => x.FullName == nameof(KnownActions.Create));
            
            Assert.Equal(KnownActions.Create, actual.KnownActionType);
            Assert.Equal(nameof(KnownActions.Create), actual.Name);
            Assert.False(actual.SupportsBulk);
            Assert.False(actual.SupportsConstraints);
            Assert.True(actual.SupportsInput);
            Assert.False(actual.SupportsLookupConditions);
            Assert.False(actual.SupportsMultipleRecordOperations);
            Assert.False(actual.SupportsRelations);
            Assert.False(actual.SupportsSequences);
        }

        [Fact]
        public void HardcodedMetadataProvider_RetrieveActionDefinitions_Always_ShouldReturnCorrectMetadataForUpdate()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveActionDefinitions()
                .Single(x => x.FullName == nameof(KnownActions.Update));
            
            Assert.Equal(KnownActions.Update, actual.KnownActionType);
            Assert.Equal(nameof(KnownActions.Update), actual.Name);
            Assert.False(actual.SupportsBulk);
            Assert.False(actual.SupportsConstraints);
            Assert.True(actual.SupportsInput);
            Assert.True(actual.SupportsLookupConditions);
            Assert.False(actual.SupportsMultipleRecordOperations);
            Assert.False(actual.SupportsRelations);
            Assert.False(actual.SupportsSequences);
        }

        [Fact]
        public void HardcodedMetadataProvider_RetrieveActionDefinitions_Always_ShouldReturnCorrectMetadataForDelete()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveActionDefinitions()
                .Single(x => x.FullName == nameof(KnownActions.Delete));
            
            Assert.Equal(KnownActions.Delete, actual.KnownActionType);
            Assert.Equal(nameof(KnownActions.Delete), actual.Name);
            Assert.False(actual.SupportsBulk);
            Assert.False(actual.SupportsConstraints);
            Assert.False(actual.SupportsInput);
            Assert.True(actual.SupportsLookupConditions);
            Assert.False(actual.SupportsMultipleRecordOperations);
            Assert.False(actual.SupportsRelations);
            Assert.False(actual.SupportsSequences);
        }

        [Fact]
        public void HardcodedMetadataProvider_RetrieveActionDefinitions_Always_ShouldReturnUniqueActionFullNames()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveActionDefinitions()
                .GroupBy(x => x.FullName);

            Assert.All(actual, a => Assert.Single(a));
        }

        [Fact]
        public void HardcodedMetadataProvider_RetrieveObjectDefinitions_Always_ShouldReturnUniqueObjectFullNames()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveObjectDefinitions()
                .GroupBy(x => x.FullName);

            Assert.All(actual, a => Assert.Single(a));
        }

        [Fact]
        public void HardcodedMetadataProvider_RetrieveObjectDefinitions_ForEachObject_ShouldUseOnlyExistingActions()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveObjectDefinitions()
                .SelectMany(x => x.SupportedActionFullNames);

            var expected = sut
                .RetrieveActionDefinitions()
                .Select(x => x.FullName);
            Assert.All(actual, action => Assert.Contains(action, expected));
        }

        [Fact]
        public void HardcodedMetadataProvider_RetrieveObjectDefinitions_ForEachObject_ShouldUseAllTheExistingActions()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveObjectDefinitions()
                .SelectMany(x => x.SupportedActionFullNames)
                .Distinct()
                .ToList();

            var expected = sut
                .RetrieveActionDefinitions()
                .Select(x => x.FullName)
                .Distinct()
                .ToList();
            Assert.Equal(expected.Count, actual.Count);
        }

        [Fact]
        public void HardcodedMetadataProvider_RetrieveObjectDefinitions_ForEachObject_ShouldReturnUniquePropertyFullNames()
        {
            var sut = CreateSystemUnderTest();

            var actual = new List<string>(
                from o in sut.RetrieveObjectDefinitions(shouldGetProperties: true)
                from p in o.PropertyDefinitions
                select $"{o.FullName} {p.FullName}");

            var expected = actual.Distinct().ToList();
            Assert.Equal(expected.Count, actual.Count);
            Assert.All(actual, a => Assert.Contains(a, expected));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HardcodedMetadataProvider_RetrieveObjectDefinitions_Always_ShouldReturnPropertiesAccordingToFlag(
            bool shouldGetProperties)
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveObjectDefinitions(shouldGetProperties: shouldGetProperties)
                .SelectMany(x => x.PropertyDefinitions);

            Assert.Equal(shouldGetProperties, actual.Any());
        }

        [Theory]
        [InlineData("Organization")]
        public void HardcodedMetadataProvider_RetrieveObjectDefinition_WithKnownObjectName_ShouldReturnObjectByName(
            string objectName)
        {
            var sut = CreateSystemUnderTest();
            var actual = sut.RetrieveObjectDefinition(objectName);
            Assert.Equal(objectName, actual.FullName);
        }

        [Theory]
        [InlineData("Organization", false)]
        [InlineData("Organization", true)]
        public void HardcodedMetadataProvider_RetrieveObjectDefinition_Always_ShouldReturnPropertiesAccordingToFlag(
            string objectName,
            bool shouldGetProperties)
        {
            var sut = CreateSystemUnderTest();

            var actual = sut
                .RetrieveObjectDefinition(objectName, shouldGetProperties)
                .PropertyDefinitions;

            Assert.Equal(shouldGetProperties, actual.Any());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HardcodedMetadataProvider_RetrieveMethodDefinitions_WhileReplicationServicesAreNotSupported_ShouldThrow(
            bool shouldGetParameters)
        {
            var sut = CreateSystemUnderTest();

            var actual = Assert.Throws<InvalidOperationException>(
                () => sut.RetrieveMethodDefinitions(shouldGetParameters));

            Assert.Contains(
                "Replication Services are not supported",
                actual.Message,
                StringComparison.InvariantCultureIgnoreCase);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void HardcodedMetadataProvider_RetrieveMethodDefinition_WhileReplicationServicesAreNotSupported_ShouldThrow(
            bool shouldGetParameters)
        {
            var sut = CreateSystemUnderTest();
            var objectName = Guid.NewGuid().ToString();

            var actual = Assert.Throws<InvalidOperationException>(
                () => sut.RetrieveMethodDefinition(objectName, shouldGetParameters));

            Assert.Contains(
                "Replication Services are not supported",
                actual.Message,
                StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void HardcodedMetadataProvider_ResetMetadata_Always_ShouldNotThrow()
        {
            var sut = CreateSystemUnderTest();
            var actual = Record.Exception(() => sut.ResetMetadata());
            Assert.Null(actual);
        }

        private static HardcodedMetadataProvider CreateSystemUnderTest() => 
            new HardcodedMetadataProvider();
    }
}
