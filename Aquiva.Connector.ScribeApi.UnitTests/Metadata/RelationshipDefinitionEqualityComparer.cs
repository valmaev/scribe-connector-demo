using System.Collections.Generic;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public sealed class RelationshipDefinitionEqualityComparer
        : IEqualityComparer<IRelationshipDefinition>
    {
        public bool Equals(IRelationshipDefinition x, IRelationshipDefinition y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;
            if (x.GetType() != y.GetType())
                return false;

            return string.Equals(x.FullName, y.FullName)
                && string.Equals(x.Name, y.Name)
                && string.Equals(x.Description, y.Description)
                && string.Equals(x.ThisObjectDefinitionFullName, y.ThisObjectDefinitionFullName)
                && string.Equals(x.RelatedObjectDefinitionFullName, y.RelatedObjectDefinitionFullName)
                && string.Equals(x.ThisProperties, y.ThisProperties)
                && string.Equals(x.RelatedProperties, y.RelatedProperties)
                && x.RelationshipType == y.RelationshipType;
        }

        public int GetHashCode(IRelationshipDefinition obj)
        {
            unchecked
            {
                var hashCode = obj.FullName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (obj.Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.ThisObjectDefinitionFullName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.RelatedObjectDefinitionFullName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.ThisProperties?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.RelatedProperties?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int) obj.RelationshipType;
                return hashCode;
            }
        }
    }
}
