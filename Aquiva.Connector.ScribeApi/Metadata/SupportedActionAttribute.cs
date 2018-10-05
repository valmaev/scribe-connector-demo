using System;
using System.ComponentModel;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    [AttributeUsage(
        validOn: AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
        AllowMultiple = true,
        Inherited = false)]
    public class SupportedActionAttribute : Attribute, IActionDefinition
    {
        public SupportedActionAttribute(KnownActions knownActionType)
        {
            if (!Enum.IsDefined(typeof(KnownActions), knownActionType))
            {
                throw new InvalidEnumArgumentException(
                    nameof(knownActionType),
                    (int) knownActionType,
                    typeof(KnownActions));
            }

            KnownActionType = knownActionType;
            FullName = knownActionType.ToString();
            Name = FullName;
            Description = FullName;
        }

        public string FullName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public KnownActions KnownActionType { get; set; }
        public bool SupportsLookupConditions { get; set; }
        public bool SupportsInput { get; set; }
        public bool SupportsBulk { get; set; }
        public bool SupportsMultipleRecordOperations { get; set; }
        public bool SupportsSequences { get; set; }
        public bool SupportsConstraints { get; set; }
        public bool SupportsRelations { get; set; }
    }
}
