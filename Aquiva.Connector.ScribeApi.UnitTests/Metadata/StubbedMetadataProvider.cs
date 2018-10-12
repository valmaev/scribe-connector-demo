using System.Collections.Generic;
using System.Linq;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class StubbedMetadataProvider : IMetadataProvider
    {
        public StubbedMetadataProvider()
        {
            ResetMetadata();
        }

        public IEnumerable<IActionDefinition> ActionDefinitions { get; set; }
        public IEnumerable<IObjectDefinition> ObjectDefinitions { get; set; }
        public IEnumerable<IMethodDefinition> MethodDefinitions { get; set; }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            return ActionDefinitions;
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            return ObjectDefinitions.Select(o => new ObjectDefinition
            {
                Description = o.Description,
                FullName = o.FullName,
                Hidden = o.Hidden,
                Name = o.Name,
                PropertyDefinitions = shouldGetProperties
                    ? o.PropertyDefinitions
                    : new List<IPropertyDefinition>(0),
                RelationshipDefinitions = shouldGetRelations
                    ? o.RelationshipDefinitions
                    : new List<IRelationshipDefinition>(0),
                SupportedActionFullNames = o.SupportedActionFullNames
            });
        }

        public IObjectDefinition RetrieveObjectDefinition(
            string objectName,
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            return ObjectDefinitions.Single(o => o.FullName == objectName);
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(
            bool shouldGetParameters = false)
        {
            return MethodDefinitions.Select(m => new MethodDefinition
            {
                Description = m.Description,
                FullName = m.FullName,
                InputObjectDefinition = shouldGetParameters
                    ? m.InputObjectDefinition
                    : null,
                Name = m.Name,
                OutputObjectDefinition = shouldGetParameters
                    ? m.OutputObjectDefinition
                    : null
            });
        }

        public IMethodDefinition RetrieveMethodDefinition(
            string objectName,
            bool shouldGetParameters = false)
        {
            return MethodDefinitions.Single(m => m.FullName == objectName);
        }

        public void ResetMetadata()
        {
            ActionDefinitions = new List<ActionDefinition>(0);
            ObjectDefinitions = new List<ObjectDefinition>(0);
            MethodDefinitions = new List<MethodDefinition>(0);
        }

        public void Dispose()
        {
        }
    }
}
