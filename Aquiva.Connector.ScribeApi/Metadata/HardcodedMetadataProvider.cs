using System;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public sealed class HardcodedMetadataProvider : IMetadataProvider
    {
        public IEnumerable<IActionDefinition> RetrieveActionDefinitions() => new[]
        {
            new ActionDefinition
            {
                FullName = nameof(KnownActions.Query),
                Name = nameof(KnownActions.Query),
                Description = nameof(KnownActions.Query),
                KnownActionType = KnownActions.Query,
                SupportsBulk = false,
                SupportsConstraints = true,
                SupportsInput = false,
                SupportsRelations = false,
                SupportsSequences = true,
                SupportsLookupConditions = true,
                SupportsMultipleRecordOperations = false
            },
            new ActionDefinition
            {
                FullName = nameof(KnownActions.Create),
                Name = nameof(KnownActions.Create),
                Description = nameof(KnownActions.Create),
                KnownActionType = KnownActions.Create,
                SupportsBulk = false,
                SupportsConstraints = false,
                SupportsInput = true,
                SupportsRelations = false,
                SupportsSequences = false,
                SupportsLookupConditions = false,
                SupportsMultipleRecordOperations = false
            },
            new ActionDefinition
            {
                FullName = nameof(KnownActions.Update),
                Name = nameof(KnownActions.Update),
                Description = nameof(KnownActions.Update),
                KnownActionType = KnownActions.Update,
                SupportsBulk = false,
                SupportsConstraints = false,
                SupportsInput = true,
                SupportsRelations = false,
                SupportsSequences = false,
                SupportsLookupConditions = true,
                SupportsMultipleRecordOperations = false
            },
            new ActionDefinition
            {
                FullName = nameof(KnownActions.Delete),
                Name = nameof(KnownActions.Delete),
                Description = nameof(KnownActions.Delete),
                KnownActionType = KnownActions.Delete,
                SupportsBulk = false,
                SupportsConstraints = false,
                SupportsInput = false,
                SupportsRelations = false,
                SupportsSequences = false,
                SupportsLookupConditions = true,
                SupportsMultipleRecordOperations = false
            }
        };

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(
            bool shouldGetProperties = false,
            bool shouldGetRelations = false) => new[]
        {
            new ObjectDefinition
            {
                FullName = "Organization",
                Name = "Organization",
                Description = "Represents the entity that is using TIBCO Scribe ® Online",
                Hidden = false,
                SupportedActionFullNames = new List<string>
                {
                    nameof(KnownActions.Query),
                    nameof(KnownActions.Create),
                    nameof(KnownActions.Update),
                    nameof(KnownActions.Delete),
                },
                PropertyDefinitions = new List<IPropertyDefinition>(0),
                RelationshipDefinitions = new List<IRelationshipDefinition>(0)
            }
        };

        public IObjectDefinition RetrieveObjectDefinition(
            string objectName,
            bool shouldGetProperties = false,
            bool shouldGetRelations = false) => null;

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(
            bool shouldGetParameters = false) => throw new InvalidOperationException();

        public IMethodDefinition RetrieveMethodDefinition(
            string objectName,
            bool shouldGetParameters = false) => throw new InvalidOperationException();

        public void ResetMetadata() { }
        public void Dispose() { }
    }
}
