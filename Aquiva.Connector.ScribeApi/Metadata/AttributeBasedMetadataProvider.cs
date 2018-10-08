using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public sealed class AttributeBasedMetadataProvider : IMetadataProvider
    {
        private readonly IEqualityComparer<IActionDefinition> _actionDefinitionComparer;

        public Assembly EntityAssembly { get; }
        public Func<Type, bool> TypeFilter { get; }

        public AttributeBasedMetadataProvider(
            Assembly entityAssembly,
            Func<Type, bool> typeFilter)
        {
            if (entityAssembly == null)
                throw new ArgumentNullException(nameof(entityAssembly));
            if (typeFilter == null)
                throw new ArgumentNullException(nameof(typeFilter));

            EntityAssembly = entityAssembly;
            TypeFilter = typeFilter;
            _actionDefinitionComparer = new FullNameAndTypeActionDefinitionEqualityComparer();
        }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            return EntityAssembly.GetTypes()
                .Where(
                    t => TypeFilter(t)
                    && t.GetCustomAttributes<ObjectDefinitionAttribute>()
                        .Any(o => !o.Hidden))
                .SelectMany(t => t.GetCustomAttributes<SupportedActionAttribute>())
                .Distinct(_actionDefinitionComparer);
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            var candidates = EntityAssembly.GetTypes()
                .Where(
                    t => TypeFilter(t)
                    && t.GetCustomAttributes<ObjectDefinitionAttribute>().Any())
                .ToDictionary(
                    keySelector: t => t.GetTypeInfo(),
                    elementSelector: t => t.GetCustomAttribute<ObjectDefinitionAttribute>());

            foreach (var candidate in candidates)
            {
                candidate.Value.FullName = string.IsNullOrWhiteSpace(candidate.Value.FullName)
                    ? candidate.Key.Name
                    : candidate.Value.FullName.Trim();
                candidate.Value.SupportedActionFullNames = candidate.Key
                    .GetCustomAttributes<SupportedActionAttribute>()
                    .Select(a => a.FullName)
                    .ToList();
                candidate.Value.PropertyDefinitions = new List<IPropertyDefinition>();

                if (!shouldGetProperties)
                {
                    continue;
                }

                var propertyCandidates = candidate.Key.GetMembers()
                    .Where(m => m.GetCustomAttributes<PropertyDefinitionAttribute>().Any())
                    .ToDictionary(
                        keySelector: m => m,
                        elementSelector: m => m.GetCustomAttribute<PropertyDefinitionAttribute>());

                foreach (var propertyCandidate in propertyCandidates)
                {
                    propertyCandidate.Value.FullName =
                        string.IsNullOrWhiteSpace(propertyCandidate.Value.FullName)
                            ? propertyCandidate.Key.Name
                            : propertyCandidate.Value.FullName.Trim();

                    FillCandidateByMemberInfo(propertyCandidate.Key, propertyCandidate.Value);
                    candidate.Value.PropertyDefinitions.Add(propertyCandidate.Value);
                }
            }

            return candidates.Values;
        }

        private static void FillCandidateByMemberInfo(MemberInfo info, IPropertyDefinition candidate)
       {
            var isProperty = info as PropertyInfo;
            if (isProperty != null)
            {
                var propertyInfo = (PropertyInfo) info;
                candidate.PropertyType = candidate.PropertyType
                    ?? (propertyInfo.PropertyType.GetPropertyDefinitionType().IsFromMscorlib()
                        ? propertyInfo.PropertyType.GetPropertyDefinitionType().FullName
                        : propertyInfo.PropertyType.GetPropertyDefinitionType().GetCustomAttributes<ObjectDefinitionAttribute>().Any()
                            ? propertyInfo.PropertyType.GetPropertyDefinitionType().GetObjectDefinitionFullName()
                            : propertyInfo.PropertyType.GetPropertyDefinitionType().FullName);
                candidate.Nullable = !candidate.IsPrimaryKey
                    && (propertyInfo.PropertyType.IsClass
                        || propertyInfo.PropertyType.IsInterface
                        || propertyInfo.PropertyType.IsNullable()
                        || !propertyInfo.PropertyType.IsValueType);
                candidate.MaxOccurs = propertyInfo.PropertyType.IsEnumerable() ? -1 : 1;
                return;
            }
           
           var isField = info as FieldInfo;
           if (isField != null)
           {
               var fieldInfo = (FieldInfo) info;
               candidate.PropertyType = candidate.PropertyType
                   ?? (fieldInfo.FieldType.GetPropertyDefinitionType().IsFromMscorlib()
                       ? fieldInfo.FieldType.GetPropertyDefinitionType().FullName
                       : fieldInfo.FieldType.GetPropertyDefinitionType().GetCustomAttributes<ObjectDefinitionAttribute>().Any()
                           ? fieldInfo.FieldType.GetPropertyDefinitionType().GetObjectDefinitionFullName()
                           : fieldInfo.FieldType.GetPropertyDefinitionType().FullName);
               candidate.Nullable = !candidate.IsPrimaryKey
                   && (fieldInfo.FieldType.IsClass
                       || fieldInfo.FieldType.IsInterface
                       || fieldInfo.FieldType.IsNullable()
                       || !fieldInfo.FieldType.IsValueType);
               candidate.MaxOccurs = fieldInfo.FieldType.IsEnumerable() ? -1 : 1;
           }
        }

        public IObjectDefinition RetrieveObjectDefinition(
            string objectName,
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            return RetrieveObjectDefinitions(shouldGetProperties, shouldGetRelations)
                .Single(o => o.FullName == objectName);
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(
            bool shouldGetParameters = false)
        {
            throw new InvalidOperationException("Replication Services are not supported");
        }

        public IMethodDefinition RetrieveMethodDefinition(
            string objectName,
            bool shouldGetParameters = false)
        {
            throw new InvalidOperationException("Replication Services are not supported");
        }

        public void ResetMetadata()
        {
        }

        public void Dispose()
        {
        }
    }
}
