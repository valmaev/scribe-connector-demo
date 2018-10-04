using System;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    [AttributeUsage(
        validOn: AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = true)]
    public class PropertyDefinitionAttribute : Attribute, IPropertyDefinition
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PropertyType { get; set; }
        public int MinOccurs { get; set; }
        public int MaxOccurs { get; set; }
        public int Size { get; set; }
        public int NumericScale { get; set; }
        public int NumericPrecision { get; set; }
        public string PresentationType { get; set; }
        public bool Nullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool UsedInQuerySelect { get; set; } = true;
        public bool UsedInQueryConstraint { get; set; }
        public bool UsedInActionInput { get; set; } = true;
        public bool UsedInActionOutput { get; set; } = true;
        public bool UsedInLookupCondition { get; set; }
        public bool UsedInQuerySequence { get; set; } = true;
        public bool RequiredInActionInput { get; set; }
    }
}
