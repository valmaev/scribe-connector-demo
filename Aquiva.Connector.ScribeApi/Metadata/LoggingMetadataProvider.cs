using System;
using System.Collections.Generic;
using Scribe.Core.Common;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public sealed class LoggingMetadataProvider : IMetadataProvider
    {
        public LoggingMetadataProvider(
            IMetadataProvider decoratedProvider,
            string connectorName)
        {
            if (decoratedProvider == null)
                throw new ArgumentNullException(nameof(decoratedProvider));
            if (connectorName == null)
                throw new ArgumentNullException(nameof(connectorName));

            DecoratedProvider = decoratedProvider;
            ConnectorName = connectorName;
        }

        public IMetadataProvider DecoratedProvider { get; }
        public string ConnectorName { get; }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            using (new LogMethodExecution(ConnectorName, nameof(RetrieveActionDefinitions)))
            {
                try
                {
                    return DecoratedProvider.RetrieveActionDefinitions();
                }
                catch (Exception ex)
                {
                    ScribeLogger.Instance.WriteException(this, ex);
                    throw;
                }
            }
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            using (new LogMethodExecution(ConnectorName, nameof(RetrieveObjectDefinitions)))
            {
                try
                {
                    return DecoratedProvider.RetrieveObjectDefinitions(
                        shouldGetProperties,
                        shouldGetRelations);
                }
                catch (Exception ex)
                {
                    ScribeLogger.Instance.WriteException(this, ex);
                    throw;
                }
            }
        }

        public IObjectDefinition RetrieveObjectDefinition(
            string objectName,
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            using (new LogMethodExecution(ConnectorName, nameof(RetrieveObjectDefinition)))
            {
                try
                {
                    return DecoratedProvider.RetrieveObjectDefinition(
                        objectName,
                        shouldGetProperties,
                        shouldGetRelations);
                }
                catch (Exception ex)
                {
                    ScribeLogger.Instance.WriteException(this, ex);
                    throw;
                }
            }
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(
            bool shouldGetParameters = false)
        {
            using (new LogMethodExecution(ConnectorName, nameof(RetrieveMethodDefinitions)))
            {
                try
                {
                    return DecoratedProvider.RetrieveMethodDefinitions(shouldGetParameters);
                }
                catch (Exception ex)
                {
                    ScribeLogger.Instance.WriteException(this, ex);
                    throw;
                }
            }
        }

        public IMethodDefinition RetrieveMethodDefinition(
            string objectName,
            bool shouldGetParameters = false)
        {
            using (new LogMethodExecution(ConnectorName, nameof(RetrieveMethodDefinition)))
            {
                try
                {
                    return DecoratedProvider.RetrieveMethodDefinition(
                        objectName,
                        shouldGetParameters);
                }
                catch (Exception ex)
                {
                    ScribeLogger.Instance.WriteException(this, ex);
                    throw;
                }
            }
        }

        public void ResetMetadata()
        {
            using (new LogMethodExecution(ConnectorName, nameof(ResetMetadata)))
            {
                try
                {
                    DecoratedProvider.ResetMetadata();
                }
                catch (Exception ex)
                {
                    ScribeLogger.Instance.WriteException(this, ex);
                    throw;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
