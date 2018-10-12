using System;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class ThrowingMetadataProvider : IMetadataProvider
    { 
        public ThrowingMetadataProvider(Exception exception)
        {
            ExceptionToThrow = exception;
        }

        public Exception ExceptionToThrow { get; }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            throw ExceptionToThrow;
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            throw ExceptionToThrow;
        }

        public IObjectDefinition RetrieveObjectDefinition(
            string objectName, 
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            throw ExceptionToThrow;
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(
            bool shouldGetParameters = false)
        {
            throw ExceptionToThrow;
        }

        public IMethodDefinition RetrieveMethodDefinition(
            string objectName, 
            bool shouldGetParameters = false)
        {
            throw ExceptionToThrow;
        }

        public void ResetMetadata()
        {
            throw ExceptionToThrow;
        }
            
        public void Dispose()
        {
        }
    }
}
