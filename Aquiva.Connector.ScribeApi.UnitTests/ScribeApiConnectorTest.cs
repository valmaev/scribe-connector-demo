using System;
using System.Linq;
using System.Reflection;
using Scribe.Core.ConnectorApi;
using Xunit;

namespace Aquiva.Connector.ScribeApi
{
    public class ScribeApiConnectorTest
    {
        [Fact]
        public void ScribeApiConnector_Always_ShouldBeDeclaredAsPublic()
        {
            var actual = typeof(ScribeApiConnector);
            Assert.True(actual.IsPublic);
        }

        [Fact]
        public void ScribeApiConnector_Always_ShouldProvideDefaultConstructorThatDoesNotThrow()
        {
            var actual = typeof(ScribeApiConnector).GetConstructor(Type.EmptyTypes);
            Assert.NotNull(actual);
            actual.Invoke(null);
        }

        [Fact]
        public void ScribeApiConnector_Always_ShouldBeTheOnlyTypeMarkedWithScribeConnectorAttribute()
        {
            var actual = typeof(ScribeApiConnector)
                .Assembly
                .GetTypes()
                .Where(x => Attribute.IsDefined(x, typeof(ScribeConnectorAttribute)));
            Assert.Single(actual);
        }

        [Fact]
        public void ScribeApiConnector_Always_ShouldHaveProperSupportedSolutionRoles()
        {
            var actual = typeof(ScribeApiConnector)
                .GetCustomAttributes<ScribeConnectorAttribute>()
                .Single()
                .SupportedSolutionRoles;

            Assert.Contains("Scribe.IS2.Source", actual);
            Assert.Contains("Scribe.IS2.Target", actual);
        }

        [Fact]
        public void ScribeApiConnector_Always_ShouldHaveProperConnectorType()
        {
            var actual = typeof(ScribeApiConnector)
                .GetCustomAttributes<ScribeConnectorAttribute>()
                .Single()
                .ConnectorType;

            Assert.Equal(typeof(ScribeApiConnector), actual);
        }

        [Fact]
        public void ScribeApiConnector_Always_ShouldSupportCloudAgents()
        {
            var actual = typeof(ScribeApiConnector)
                .GetCustomAttributes<ScribeConnectorAttribute>()
                .Single()
                .SupportsCloud;

            Assert.True(actual);
        }

        [Fact]
        public void ScribeApiConnector_Always_ShouldUseGenericConnectionUI()
        {
            var actual = typeof(ScribeApiConnector)
                .GetCustomAttributes<ScribeConnectorAttribute>()
                .Single();

            Assert.Equal("ScribeOnline.GenericConnectionUI", actual.ConnectionUITypeName);
            Assert.Equal("1.0.0.0", actual.ConnectionUIVersion);
        }

        [Fact]
        public void ScribeApiConnector_Always_ShouldNotUseSettingsUI()
        {
            var actual = typeof(ScribeApiConnector)
                .GetCustomAttributes<ScribeConnectorAttribute>()
                .Single();

            Assert.Empty(actual.SettingsUITypeName);
            Assert.Equal("1.0.0.0", actual.SettingsUIVersion);
            Assert.Empty(actual.XapFileName);
        }
    }
}
