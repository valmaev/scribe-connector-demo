using Aquiva.Connector.ScribeApi.Metadata;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Domain
{
    [ObjectDefinition]
    [ActionDefinition(KnownActions.Query)]
    [ActionDefinition(KnownActions.Create)]
    [ActionDefinition(KnownActions.Update)]
    [ActionDefinition(KnownActions.Delete)]
    public class Organization
    {
        [PropertyDefinition(
            UsedInActionOutput = true,
            UsedInActionInput = false,
            RequiredInActionInput = false,
            UsedInQueryConstraint = true,
            UsedInLookupCondition = false,
            IsPrimaryKey = true)]
        public int? Id { get; set; }

        [PropertyDefinition(
            UsedInActionOutput = true,
            UsedInActionInput = true,
            RequiredInActionInput = true,
            UsedInQueryConstraint = true)]
        public string Name { get; set; }

        [PropertyDefinition(
            UsedInActionOutput = true,
            UsedInActionInput = true,
            RequiredInActionInput = true,
            UsedInQueryConstraint = true)]
        public int? ParentId { get; set; }
    }
}
