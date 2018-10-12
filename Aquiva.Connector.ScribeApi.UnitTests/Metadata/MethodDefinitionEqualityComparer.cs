using System.Collections.Generic;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class MethodDefinitionEqualityComparer : IEqualityComparer<IMethodDefinition>
    {
        private readonly IEqualityComparer<IObjectDefinition> _objectDefinitionComparer =
            new ObjectDefinitionEqualityComparer();

        public bool Equals(IMethodDefinition x, IMethodDefinition y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null))
                return false;
            if (ReferenceEquals(y, null))
                return false;
            if (x.GetType() != y.GetType())
                return false;

            return string.Equals(x.Name, y.Name)
                && string.Equals(x.FullName, y.FullName)
                && string.Equals(x.Description, y.Description)
                && _objectDefinitionComparer.Equals(
                    x.InputObjectDefinition,
                    y.InputObjectDefinition)
                && _objectDefinitionComparer.Equals(
                    x.OutputObjectDefinition,
                    y.OutputObjectDefinition);
        }

        public int GetHashCode(IMethodDefinition obj)
        {
            unchecked
            {
                var hashCode = obj.Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (obj.FullName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.InputObjectDefinition?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.OutputObjectDefinition?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
