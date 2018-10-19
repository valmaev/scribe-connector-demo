using System;
using System.Diagnostics;
using System.Linq;
using AutoFixture;
using AutoFixture.Idioms;
using Scribe.Core.ConnectorApi;
using Xunit;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    [Collection(nameof(LoggingCollection))]
    public class LoggingMetadataProviderTest
    {
        private readonly LoggingFixture _fixture;

        public LoggingMetadataProviderTest(LoggingFixture fixture)
        {
            _fixture = fixture;
            _fixture.StubbedLogWriter.LogEntries.Clear();
        }

        [Theory, AutoMetaData]
        public void LoggingMetadataProvider_AllPublicConstructors_Always_ShouldHaveNullGuards(
            Fixture fixture,
            StubbedMetadataProvider decoratedProvider,
            GuardClauseAssertion assertion)
        {
            fixture.Inject<IMetadataProvider>(decoratedProvider);
            assertion.Verify(typeof(LoggingMetadataProvider).GetConstructors());
        }

        [Theory, AutoMetaData]
        public void LoggingMetadataProvider_RetrieveActionDefinitions_Always_ShouldReturnResultFromDecoratedProvider(
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = decoratedProvider.RetrieveActionDefinitions().ToList();
            
            var actual = sut.RetrieveActionDefinitions().ToList();
            
            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }

        [Theory] 
        [InlineAutoMetaData(false, false)]
        [InlineAutoMetaData(false, true)]
        [InlineAutoMetaData(true, false)]
        [InlineAutoMetaData(true, true)]
        public void LoggingMetadataProvider_RetrieveObjectDefinitions_Always_ShouldReturnResultFromDecoratedProvider(
            bool shouldGetProperties,
            bool shouldGetRelations,
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = decoratedProvider
                .RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations)
                .ToList();
            
            var actual = sut
                .RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations)
                .ToList();
            
            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actual[i], new ObjectDefinitionEqualityComparer());
            }
        }

        [Theory] 
        [InlineAutoMetaData(false, false)]
        [InlineAutoMetaData(false, true)]
        [InlineAutoMetaData(true, false)]
        [InlineAutoMetaData(true, true)]
        public void LoggingMetadataProvider_RetrieveObjectDefinition_Always_ShouldReturnResultFromDecoratedProvider(
            bool shouldGetProperties,
            bool shouldGetRelations,
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var name = decoratedProvider.ObjectDefinitions.First().FullName;
            var expected = decoratedProvider
                .RetrieveObjectDefinition(name, shouldGetProperties, shouldGetRelations);

            var actual = sut
                .RetrieveObjectDefinition(name, shouldGetProperties, shouldGetRelations);
            
            Assert.Equal(expected, actual, new ObjectDefinitionEqualityComparer());
        }

        [Theory] 
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void LoggingMetadataProvider_RetrieveMethodDefinitions_Always_ShouldReturnResultFromDecoratedProvider(
            bool shouldGetParameters,
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var expected = decoratedProvider
                .RetrieveMethodDefinitions(shouldGetParameters)
                .ToList();
            
            var actual = sut
                .RetrieveMethodDefinitions(shouldGetParameters)
                .ToList();
            
            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actual[i], new MethodDefinitionEqualityComparer());
            }
        }

        [Theory] 
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void LoggingMetadataProvider_RetrieveMethodDefinition_Always_ShouldReturnResultFromDecoratedProvider(
            bool shouldGetParameters,
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var name = decoratedProvider.MethodDefinitions.First().FullName;
            var expected = decoratedProvider
                .RetrieveMethodDefinition(name, shouldGetParameters);
            
            var actual = sut
                .RetrieveMethodDefinition(name, shouldGetParameters);
            
            Assert.Equal(expected, actual, new MethodDefinitionEqualityComparer());
        }

        [Theory, AutoMetaData]
        public void LoggingMetadataProvider_ResetMetadata_Always_ShouldCallDecoratedProvider(
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            Assert.NotEmpty(decoratedProvider.ActionDefinitions);
            Assert.NotEmpty(decoratedProvider.ObjectDefinitions);
            Assert.NotEmpty(decoratedProvider.MethodDefinitions);

            sut.ResetMetadata();
            
            Assert.Empty(decoratedProvider.ActionDefinitions);
            Assert.Empty(decoratedProvider.ObjectDefinitions);
            Assert.Empty(decoratedProvider.MethodDefinitions); 
        }

        [Theory, AutoMetaData]
        public void LoggingMetadataProvider_RetrieveActionDefinitions_Always_ShouldLogBeforeAndAfterDecoratedMethodExecution(
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            sut.RetrieveActionDefinitions();
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveActionDefinitions), actual.First().Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveActionDefinitions), actual.Last().Message);
        }

        [Theory]
        [InlineAutoMetaData(false, false)]
        [InlineAutoMetaData(false, true)]
        [InlineAutoMetaData(true, false)]
        [InlineAutoMetaData(true, true)]
        public void LoggingMetadataProvider_RetrieveObjectDefinitions_Always_ShouldLogBeforeAndAfterDecoratedMethodExecution(
            bool shouldGetProperties,
            bool shouldGetRelations,
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            sut.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations);
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinitions), actual.First().Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinitions), actual.Last().Message);
        }

        [Theory]
        [InlineAutoMetaData(false, false)]
        [InlineAutoMetaData(false, true)]
        [InlineAutoMetaData(true, false)]
        [InlineAutoMetaData(true, true)]
        public void LoggingMetadataProvider_RetrieveObjectDefinition_Always_ShouldLogBeforeAndAfterDecoratedMethodExecution(
            bool shouldGetProperties,
            bool shouldGetRelations,
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var name = decoratedProvider.ObjectDefinitions.First().FullName;
            
            sut.RetrieveObjectDefinition(name, shouldGetProperties, shouldGetRelations);
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinition), actual.First().Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinition), actual.Last().Message);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void LoggingMetadataProvider_RetrieveMethodDefinitions_Always_ShouldLogBeforeAndAfterDecoratedMethodExecution(
            bool shouldGetParameters,
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            sut.RetrieveMethodDefinitions(shouldGetParameters);
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinitions), actual.First().Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinitions), actual.Last().Message);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void LoggingMetadataProvider_RetrieveMethodDefinition_Always_ShouldLogBeforeAndAfterDecoratedMethodExecution(
            bool shouldGetParameters,
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var name = decoratedProvider.MethodDefinitions.First().FullName;
            
            sut.RetrieveMethodDefinition(name, shouldGetParameters);
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinition), actual.First().Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinition), actual.Last().Message);
        }

        [Theory, AutoMetaData]
        public void LoggingMetadataProvider_ResetMetadata_Always_ShouldLogBeforeAndAfterDecoratedMethodExecution(
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            sut.ResetMetadata();
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.ResetMetadata), actual.First().Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.ResetMetadata), actual.Last().Message);
        }

        [Theory, AutoMetaData]
        public void LoggingMetadataProvider_RetrieveActionDefinitions_WhenDecoratedProviderThrowsException_ShouldLogItAndRethrow(
            ThrowingMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actualException = Assert.Throws<Exception>(
                () => sut.RetrieveActionDefinitions());
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveActionDefinitions), actual.First().Message);
            
            Assert.Equal(TraceEventType.Error, actual[1].Severity);
            Assert.Contains(actualException.Message, actual[1].Message);
            Assert.Contains(nameof(sut.RetrieveActionDefinitions), actual[1].Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveActionDefinitions), actual.Last().Message);
        }

        [Theory]
        [InlineAutoMetaData(false, false)]
        [InlineAutoMetaData(false, true)]
        [InlineAutoMetaData(true, false)]
        [InlineAutoMetaData(true, true)]
        public void LoggingMetadataProvider_RetrieveObjectDefinitions_WhenDecoratedProviderThrowsException_ShouldLogItAndRethrow(
            bool shouldGetProperties,
            bool shouldGetRelations,
            ThrowingMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actualException = Assert.Throws<Exception>(
                () => sut.RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations));
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinitions), actual.First().Message);
            
            Assert.Equal(TraceEventType.Error, actual[1].Severity);
            Assert.Contains(actualException.Message, actual[1].Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinitions), actual[1].Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinitions), actual.Last().Message);
        }

        [Theory]
        [InlineAutoMetaData(false, false)]
        [InlineAutoMetaData(false, true)]
        [InlineAutoMetaData(true, false)]
        [InlineAutoMetaData(true, true)]
        public void LoggingMetadataProvider_RetrieveObjectDefinition_WhenDecoratedProviderThrowsException_ShouldLogItAndRethrow(
            bool shouldGetProperties,
            bool shouldGetRelations,
            string anyValue,
            ThrowingMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actualException = Assert.Throws<Exception>(
                () => sut.RetrieveObjectDefinition(anyValue, shouldGetProperties, shouldGetRelations));
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinition), actual.First().Message);
            
            Assert.Equal(TraceEventType.Error, actual[1].Severity);
            Assert.Contains(actualException.Message, actual[1].Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinition), actual[1].Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveObjectDefinition), actual.Last().Message);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void LoggingMetadataProvider_RetrieveMethodDefinitions_WhenDecoratedProviderThrowsException_ShouldLogItAndRethrow(
            bool shouldGetParameters,
            ThrowingMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actualException = Assert.Throws<Exception>(
                () => sut.RetrieveMethodDefinitions(shouldGetParameters));
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinitions), actual.First().Message);
            
            Assert.Equal(TraceEventType.Error, actual[1].Severity);
            Assert.Contains(actualException.Message, actual[1].Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinitions), actual[1].Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinitions), actual.Last().Message);
        }

        [Theory]
        [InlineAutoMetaData(false)]
        [InlineAutoMetaData(true)]
        public void LoggingMetadataProvider_RetrieveMethodDefinition_WhenDecoratedProviderThrowsException_ShouldLogItAndRethrow(
            bool shouldGetParameters,
            string anyValue,
            ThrowingMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actualException = Assert.Throws<Exception>(
                () => sut.RetrieveMethodDefinition(anyValue, shouldGetParameters));
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinition), actual.First().Message);
            
            Assert.Equal(TraceEventType.Error, actual[1].Severity);
            Assert.Contains(actualException.Message, actual[1].Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinition), actual[1].Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.RetrieveMethodDefinition), actual.Last().Message);
        }
        
        [Theory, AutoMetaData]
        public void LoggingMetadataProvider_ResetMetadata_WhenDecoratedProviderThrowsException_ShouldLogItAndRethrow(
            ThrowingMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actualException = Assert.Throws<Exception>(() => sut.ResetMetadata());
            
            var actual = _fixture.StubbedLogWriter.LogEntries;
            Assert.Equal(TraceEventType.Verbose, actual.First().Severity);
            Assert.Contains(sut.ConnectorName, actual.First().Title);
            Assert.Contains("Entering", actual.First().Message);
            Assert.Contains(nameof(sut.ResetMetadata), actual.First().Message);
            
            Assert.Equal(TraceEventType.Error, actual[1].Severity);
            Assert.Contains(actualException.Message, actual[1].Message);
            Assert.Contains(nameof(sut.ResetMetadata), actual[1].Message);
            
            Assert.Equal(TraceEventType.Verbose, actual.Last().Severity);
            Assert.Contains(sut.ConnectorName, actual.Last().Title);
            Assert.Contains("Leaving", actual.Last().Message);
            Assert.Contains(nameof(sut.ResetMetadata), actual.Last().Message);
        }
        
        [Theory, AutoMetaData]
        public void LoggingMetadataProvider_Dispose_Always_ShouldNotThrow(
            StubbedMetadataProvider decoratedProvider)
        {
            var sut = CreateSystemUnderTest(decoratedProvider);
            var actual = Record.Exception(() => sut.Dispose());
            Assert.Null(actual);
        }

        private static LoggingMetadataProvider CreateSystemUnderTest(
            IMetadataProvider decoratedProvider = null,
            string connectorName = "FooConnector")
        {
            return new LoggingMetadataProvider(
                decoratedProvider ?? new StubbedMetadataProvider(),
                connectorName);
        }
    }
}
