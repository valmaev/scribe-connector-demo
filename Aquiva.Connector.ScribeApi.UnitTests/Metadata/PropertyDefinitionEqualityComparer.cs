using System.Collections.Generic;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public sealed class PropertyDefinitionAttributeEqualityComparer
        : IEqualityComparer<IPropertyDefinition>
    {
        public bool Equals(IPropertyDefinition x, IPropertyDefinition y)
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
                && string.Equals(x.PropertyType, y.PropertyType)
                && x.MinOccurs == y.MinOccurs
                && x.MaxOccurs == y.MaxOccurs
                && x.Size == y.Size
                && x.NumericScale == y.NumericScale
                && x.NumericPrecision == y.NumericPrecision
                && string.Equals(x.PresentationType, y.PresentationType)
                && x.Nullable == y.Nullable
                && x.IsPrimaryKey == y.IsPrimaryKey
                && x.UsedInQuerySelect == y.UsedInQuerySelect
                && x.UsedInQueryConstraint == y.UsedInQueryConstraint
                && x.UsedInActionInput == y.UsedInActionInput
                && x.UsedInActionOutput == y.UsedInActionOutput
                && x.UsedInLookupCondition == y.UsedInLookupCondition
                && x.UsedInQuerySequence == y.UsedInQuerySequence
                && x.RequiredInActionInput == y.RequiredInActionInput;
        }

        public int GetHashCode(IPropertyDefinition obj)
        {
            unchecked
            {
                var hashCode = obj.FullName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (obj.Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (obj.PropertyType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ obj.MinOccurs;
                hashCode = (hashCode * 397) ^ obj.MaxOccurs;
                hashCode = (hashCode * 397) ^ obj.Size;
                hashCode = (hashCode * 397) ^ obj.NumericScale;
                hashCode = (hashCode * 397) ^ obj.NumericPrecision;
                hashCode = (hashCode * 397) ^ (obj.PresentationType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ obj.Nullable.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.IsPrimaryKey.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.UsedInQuerySelect.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.UsedInQueryConstraint.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.UsedInActionInput.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.UsedInActionOutput.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.UsedInLookupCondition.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.UsedInQuerySequence.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.RequiredInActionInput.GetHashCode();
                return hashCode;
            }
        }
    }
}
