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

        private static HardcodedMetadataProvider CreateSystemUnderTest() => 
            new HardcodedMetadataProvider();
    }
}