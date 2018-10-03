using System;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    [AttributeUsage(
        validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
        AllowMultiple = false,
        Inherited = false)]
    public class ObjectDefinitionAttribute : Attribute, IObjectDefinition
    {
        public string FullName { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Hidden { get; set; }
        public List<string> SupportedActionFullNames { get; set; }
        public List<IPropertyDefinition> PropertyDefinitions { get; set; }
        public List<IRelationshipDefinition> RelationshipDefinitions { get; set; }
    }
}
