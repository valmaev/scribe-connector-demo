using System;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Scribe.Core.ConnectorApi;
using Xunit;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class CachingMetadataProviderTest
    {
        [Theory, AutoData]
        public void CachingMetadataProvider_AllPublicConstructors_Always_ShouldHaveNullGuards(
            [Frozen] Fixture fixture,
            GuardClauseAssertion assertion)
        {
            fixture.Inject<IMetadataProvider>(
                new AttributeBasedMetadataProvider(
                    Assembly.GetExecutingAssembly(),
                    t => false));
            assertion.Verify(typeof(CachingMetadataProvider).GetConstructors());
        }

        [Theory, AutoMetaData]
        public void CachingMetadataProvider_RetrieveActionDefinitions_Always_ShouldCacheOnFirstCall(
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider)
        {
            // Arrange
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = decoratedProvider.RetrieveActionDefinitions().ToList();

            // Act 1: First call should cache result for all next calls
            var actual = sut.RetrieveActionDefinitions().ToList();

            // Assert
            Assert.Equal(expected, actual);

            decoratedProvider.ResetMetadata();
            Assert.Empty(decoratedProvider.RetrieveActionDefinitions());

            actual = sut.RetrieveActionDefinitions().ToList();
            Assert.Equal(expected, actual);

            // Act 2: CachingMetadataProvider.ResetMetadata() call should lead to cache reset
            sut.ResetMetadata();

            // Assert
            actual = sut.RetrieveActionDefinitions().ToList();
            Assert.Empty(actual);
        }

        [Theory]
        [InlineAutoMetaData(false, false)]
        [InlineAutoMetaData(false, true)]
        [InlineAutoMetaData(true, false)]
        [InlineAutoMetaData(true, true)]
        public void CachingMetadataProvider_RetrieveObjectDefinitions_Always_ShouldCacheOnFirstCall(
            bool shouldGetProperties,
            bool shouldGetRelations,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider,
            ObjectDefinitionEqualityComparer comparer)
        {
            // Arrange
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = decoratedProvider.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations).ToList();

            // Act 1: First call should cache result for all next calls
            var actual = sut.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations).ToList();

            // Assert
            Assert.Equal(expected, actual, comparer);

            decoratedProvider.ResetMetadata();
            Assert.Empty(decoratedProvider.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations));

            actual = sut.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations).ToList();
            Assert.Equal(expected, actual, comparer);

            // Act 2: CachingMetadataProvider.ResetMetadata() call should lead to cache reset
            sut.ResetMetadata();

            // Assert
            actual = sut.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations).ToList();
            Assert.Empty(actual);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void CachingMetadataProvider_RetrieveObjectDefinitions_Always_ShouldHonorShouldGetProperties(
            bool shouldGetProperties,
            bool shouldGetRelations,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actual = sut.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations).ToList();
            actual.ForEach(a => Assert.Equal(shouldGetProperties, a.PropertyDefinitions.Any()));
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void CachingMetadataProvider_RetrieveObjectDefinitions_Always_ShouldHonorShouldGetRelations(
            bool shouldGetRelations,
            bool shouldGetProperties,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actual = sut.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations).ToList();
            actual.ForEach(a => Assert.Equal(shouldGetRelations, a.RelationshipDefinitions.Any()));
        }

        [Theory]
        [InlineAutoMetaData(false, false)]
        [InlineAutoMetaData(false, true)]
        [InlineAutoMetaData(true, false)]
        [InlineAutoMetaData(true, true)]
        public void CachingMetadataProvider_RetrieveObjectDefinition_Always_ShouldCacheOnFirstCall(
            bool shouldGetProperties,
            bool shouldGetRelations,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider,
            ObjectDefinitionEqualityComparer comparer)
        {
            // Arrange
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = decoratedProvider
                .RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations)
                .First();

            // Act 1: First call should cache result for all next calls
            var actual = sut.RetrieveObjectDefinition(
                expected.FullName,
                shouldGetProperties,
                shouldGetRelations);

            // Assert
            Assert.Equal(expected, actual, comparer);

            decoratedProvider.ResetMetadata();
            Assert.Empty(decoratedProvider.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations));

            actual = sut.RetrieveObjectDefinition(
                expected.FullName,
                shouldGetProperties,
                shouldGetRelations);
            Assert.Equal(expected, actual, comparer);

            // Act 2: CachingMetadataProvider.ResetMetadata() call should lead to cache reset
            sut.ResetMetadata();

            // Assert
            var actualException = Assert.Throws<ArgumentException>(
                () => sut.RetrieveObjectDefinition(
                    expected.FullName,
                    shouldGetProperties,
                    shouldGetRelations));
            Assert.Contains(expected.FullName, actualException.Message, StringComparison.InvariantCulture);
            Assert.Equal("objectName", actualException.ParamName);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void CachingMetadataProvider_RetrieveObjectDefinition_Always_ShouldHonorShouldGetProperties(
            bool shouldGetProperties,
            bool shouldGetRelations,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var objectName = decoratedProvider.ObjectDefinitions.First().FullName;

            var actual = sut.RetrieveObjectDefinition(objectName, shouldGetProperties, shouldGetRelations);

            Assert.Equal(shouldGetProperties, actual.PropertyDefinitions.Any());
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void CachingMetadataProvider_RetrieveObjectDefinition_Always_ShouldHonorShouldGetRelations(
            bool shouldGetRelations,
            bool shouldGetProperties,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var objectName = decoratedProvider.ObjectDefinitions.First().FullName;

            var actual = sut.RetrieveObjectDefinition(objectName, shouldGetProperties, shouldGetRelations);

            Assert.Equal(shouldGetRelations, actual.RelationshipDefinitions.Any());
        }

        [Theory]
        [InlineAutoMetaData(false, false)]
        [InlineAutoMetaData(false, true)]
        [InlineAutoMetaData(true, false)]
        [InlineAutoMetaData(true, true)]
        public void CachingMetadataProvider_RetrieveObjectDefinition_Always_ShouldBeInSyncWithRetrieveObjectDefinitionsResult(
            bool shouldGetProperties,
            bool shouldGetRelations,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider,
            ObjectDefinitionEqualityComparer comparer)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = sut
                .RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations)
                .First();

            var actual = sut.RetrieveObjectDefinition(
                expected.FullName,
                shouldGetProperties,
                shouldGetRelations);

            Assert.Equal(expected, actual, comparer);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void CachingMetadataProvider_RetrieveMethodDefinitions_Always_ShouldCacheOnFirstCall(
            bool shouldGetParameters,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider,
            MethodDefinitionEqualityComparer comparer)
        {
            // Arrange
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = decoratedProvider.RetrieveMethodDefinitions(shouldGetParameters).ToList();

            // Act 1: First call should cache result for all next calls
            var actual = sut.RetrieveMethodDefinitions(shouldGetParameters).ToList();

            // Assert
            Assert.Equal(expected, actual, comparer);

            decoratedProvider.ResetMetadata();
            Assert.Empty(decoratedProvider.RetrieveMethodDefinitions(shouldGetParameters));

            actual = sut.RetrieveMethodDefinitions(shouldGetParameters).ToList();
            Assert.Equal(expected, actual, comparer);

            // Act 2: CachingMetadataProvider.ResetMetadata() call should lead to cache reset
            sut.ResetMetadata();

            // Assert
            actual = sut.RetrieveMethodDefinitions(shouldGetParameters).ToList();
            Assert.Empty(actual);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void CachingMetadataProvider_RetrieveMethodDefinitions_Always_ShouldHonorShouldGetParameters(
            bool shouldGetParameters,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actual = sut.RetrieveMethodDefinitions(shouldGetParameters).ToList();

            actual.ForEach(a =>
            {
                Assert.Equal(shouldGetParameters, a.InputObjectDefinition != null);
                Assert.Equal(shouldGetParameters, a.OutputObjectDefinition != null);
            });
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void CachingMetadataProvider_RetrieveMethodDefinition_Always_ShouldCacheOnFirstCall(
            bool shouldGetParameters,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider,
            MethodDefinitionEqualityComparer comparer)
        {
            // Arrange
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = sut
                .RetrieveMethodDefinitions(shouldGetParameters)
                .First();

            // Act 1: First call should cache result for all next calls
            var actual = sut.RetrieveMethodDefinition(expected.FullName, shouldGetParameters);

            // Assert
            Assert.Equal(expected, actual, comparer);

            decoratedProvider.ResetMetadata();
            Assert.Empty(decoratedProvider.RetrieveMethodDefinitions(shouldGetParameters));

            actual = sut.RetrieveMethodDefinition(expected.FullName, shouldGetParameters);
            Assert.Equal(expected, actual, comparer);

            // Act 2: CachingMetadataProvider.ResetMetadata() call should lead to cache reset
            sut.ResetMetadata();

            // Assert
            var actualException = Assert.Throws<ArgumentException>(
                () => sut.RetrieveMethodDefinition(expected.FullName, shouldGetParameters));
            Assert.Contains(expected.FullName, actualException.Message, StringComparison.InvariantCulture);
            Assert.Equal("objectName", actualException.ParamName);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void CachingMetadataProvider_RetrieveMethodDefinition_Always_ShouldHonorShouldGetParameters(
            bool shouldGetParameters,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var objectName = decoratedProvider.MethodDefinitions.First().FullName;

            var actual = sut.RetrieveMethodDefinition(objectName, shouldGetParameters);

            Assert.Equal(shouldGetParameters, actual.InputObjectDefinition != null);
            Assert.Equal(shouldGetParameters, actual.OutputObjectDefinition != null);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void CachingMetadataProvider_RetrieveMethodDefinition_Always_ShouldBeInSyncWithRetrieveMethodDefinitionsResult(
            bool shouldGetParameters,
            [Frozen(Matching.ImplementedInterfaces)] StubbedMetadataProvider decoratedProvider,
            MethodDefinitionEqualityComparer comparer)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = sut
                .RetrieveMethodDefinitions(shouldGetParameters)
                .First();

            var actual = sut.RetrieveMethodDefinition(
                expected.FullName,
                shouldGetParameters);

            Assert.Equal(expected, actual, comparer);
        }

        [Fact]
        public void CachingMetadataProvider_Dispose_Always_ShouldNotThrow()
        {
            var sut = CreateSystemUnderTest();
            var actual = Record.Exception(() => sut.Dispose());
            Assert.Null(actual);
        }

        private static CachingMetadataProvider CreateSystemUnderTest(
            IMetadataProvider decoratedProvider = null)
        {
            return new CachingMetadataProvider(
                decoratedProvider ?? new StubbedMetadataProvider());
        }
    }
}
