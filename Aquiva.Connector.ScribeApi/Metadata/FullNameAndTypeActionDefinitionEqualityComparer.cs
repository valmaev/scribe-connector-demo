using System.Collections.Generic;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public sealed class FullNameAndTypeActionDefinitionEqualityComparer
        : IEqualityComparer<IActionDefinition>
    {
        public bool Equals(IActionDefinition x, IActionDefinition y)
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
                && string.Equals(x.Description, y.Description);
        }

        public int GetHashCode(IActionDefinition obj)
        {
            unchecked
            {
                var hashCode = obj.FullName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (obj.Description?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
