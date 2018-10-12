using System.Collections.Generic;
using System.Linq;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public sealed class ObjectDefinitionEqualityComparer
        : IEqualityComparer<IObjectDefinition>
    {
        public bool Equals(IObjectDefinition x, IObjectDefinition y)
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
                && x.Hidden == y.Hidden
                && ((x.SupportedActionFullNames == null && y.SupportedActionFullNames == null)
                    || x.SupportedActionFullNames?.SequenceEqual(y.SupportedActionFullNames) == true)
                && ((x.PropertyDefinitions == null && y.PropertyDefinitions == null)
                    || x.PropertyDefinitions?.SequenceEqual(
                       y.PropertyDefinitions,
                       new PropertyDefinitionAttributeEqualityComparer()) == true)
                && ((x.PropertyDefinitions == null && y.RelationshipDefinitions == null)
                    || x.RelationshipDefinitions?.SequenceEqual(
                       y.RelationshipDefinitions,
                       new RelationshipDefinitionEqualityComparer()) == true);
        }

        public int GetHashCode(IObjectDefinition obj)
        {
            unchecked
            {
                var hashCode = obj.FullName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (obj.Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ obj.Hidden.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.SupportedActionFullNames?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.PropertyDefinitions?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.RelationshipDefinitions?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
