using System;
using System.Collections.Generic;
using System.Linq;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public sealed class CachingMetadataProvider : IMetadataProvider
    {
        private IReadOnlyCollection<IActionDefinition> _actionCache;
        private IDictionary<string, IObjectDefinition> _objectCache;
        private IDictionary<string, IMethodDefinition> _methodCache;

        public CachingMetadataProvider(IMetadataProvider decoratedProvider)
        {
            if (decoratedProvider == null)
                throw new ArgumentNullException(nameof(decoratedProvider));

            DecoratedProvider = decoratedProvider;
            ResetMetadata();
        }
        
        public IMetadataProvider DecoratedProvider { get; }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            if (!_actionCache.Any())
            {
                _actionCache = DecoratedProvider.RetrieveActionDefinitions().ToList();
            }
            return _actionCache;
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            if (!_objectCache.Any())
            {
                WarmUpObjectCache();
            }

            return shouldGetProperties && shouldGetRelations
                ? _objectCache.Values
                : _objectCache.Values.Select(
                    o => Copy(o, shouldGetProperties, shouldGetRelations));
        }

        private static IObjectDefinition Copy(
            IObjectDefinition source,
            bool shouldGetProperties,
            bool shouldGetRelations)
        {
            return new ObjectDefinition
            {
                Description = source.Description,
                FullName = source.FullName,
                Hidden = source.Hidden,
                Name = source.Name,
                PropertyDefinitions = shouldGetProperties
                    ? source.PropertyDefinitions
                    : new List<IPropertyDefinition>(0),
                RelationshipDefinitions = shouldGetRelations
                    ? source.RelationshipDefinitions
                    : new List<IRelationshipDefinition>(0),
                SupportedActionFullNames = source.SupportedActionFullNames
            };
        }

        private void WarmUpObjectCache()
        {
            _objectCache = DecoratedProvider
                .RetrieveObjectDefinitions(
                    shouldGetProperties: true,
                    shouldGetRelations: true)
                .ToDictionary(
                    keySelector: o => o.FullName);
        }

        public IObjectDefinition RetrieveObjectDefinition(
            string objectName,
            bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            if (!_objectCache.Any())
            {
                WarmUpObjectCache();
            }

            if (!_objectCache.ContainsKey(objectName))
            {
                throw new ArgumentException(
                    $"ObjectDefinition with {objectName} name was not found",
                    nameof(objectName));
            }

            return shouldGetProperties && shouldGetRelations
                ? _objectCache[objectName]
                : Copy(
                    _objectCache[objectName],
                    shouldGetProperties,
                    shouldGetRelations);
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(
            bool shouldGetParameters = false)
        {
            if (!_methodCache.Any())
            {
                WarmUpMethodCache();
            }

            return shouldGetParameters
                ? _methodCache.Values
                : _methodCache.Values.Select(m => Copy(m, false));
        }

        private static IMethodDefinition Copy(
            IMethodDefinition source,
            bool shouldGetParameters)
        {
            return new MethodDefinition
            {
                Description = source.Description,
                FullName = source.FullName,
                InputObjectDefinition = shouldGetParameters
                    ? source.InputObjectDefinition
                    : null,
                OutputObjectDefinition = shouldGetParameters
                    ? source.OutputObjectDefinition
                    : null,
                Name = source.Name
            };
        }

        private void WarmUpMethodCache()
        {
            _methodCache = DecoratedProvider
                .RetrieveMethodDefinitions(shouldGetParameters: true)
                .ToDictionary(keySelector: m => m.FullName);
        }

        public IMethodDefinition RetrieveMethodDefinition(
            string objectName,
            bool shouldGetParameters = false)
        {
            if (!_methodCache.Any())
            {
                WarmUpMethodCache();
            }

            if (!_methodCache.ContainsKey(objectName))
            {
                throw new ArgumentException(
                    $"Can't find object definition for {objectName} type",
                    nameof(objectName));
            }

            return shouldGetParameters
                ? _methodCache[objectName]
                : Copy(_methodCache[objectName], false);
        }

        public void ResetMetadata()
        {
            _actionCache = new List<IActionDefinition>(0);
            _objectCache = new Dictionary<string, IObjectDefinition>(0);
            _methodCache = new Dictionary<string, IMethodDefinition>(0);
        }

        public void Dispose()
        {
        }
    }
}
