using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using AutoFixture.Idioms;
using Scribe.Core.ConnectorApi.Cryptography;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.ConnectionUI;
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

        [Fact]
        public void ScribeApiConnector_Always_ShouldImplementIConnectorInterface()
        {
            var actual = typeof(ScribeApiConnector).GetInterfaces();
            Assert.Contains(typeof(IConnector), actual);
        }

        [Theory, ScribeApiConnectorData]
        public void ScribeApiConnector_AllPublicConstructors_Always_ShouldHaveNullGuards(
            GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(ScribeApiConnector).GetConstructors());
        }

        [Fact]
        public void ScribeApiConnector_ConnectorTypeId_Always_ShouldReturnParsedIdFromTheAttribute()
        {
            var sut = CreateSystemUnderTest();
            var expected = sut
                .GetType()
                .GetCustomAttributes<ScribeConnectorAttribute>()
                .Single()
                .ConnectorTypeId;
            Assert.Equal(Guid.Parse(expected), sut.ConnectorTypeId);
        }

        [Fact]
        public void ScribeApiConnector_PreConnect_Always_ShouldReturnSerializedFormWithDefinedCryptoKey()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut.PreConnect(new Dictionary<string, string>(0));
            var actualForm = FormDefinition.Deserialize(actual);

            Assert.NotEmpty(actualForm.CryptoKey);
        }

        [Fact]
        public void ScribeApiConnector_PreConnect_Always_ShouldReturnSerializedFormWithDefinedHelpUri()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut.PreConnect(new Dictionary<string, string>(0));
            var actualForm = FormDefinition.Deserialize(actual);

            Assert.True(actualForm.HelpUri.IsAbsoluteUri);
            Assert.Equal(new Uri("https://aquivalabs.com/"), actualForm.HelpUri);
        }

        [Fact]
        public void ScribeApiConnector_PreConnect_Always_ShouldReturnSerializedFormWithDefinedCompanyName()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut.PreConnect(new Dictionary<string, string>(0));
            var actualForm = FormDefinition.Deserialize(actual);

            Assert.Equal("Aquiva Labs", actualForm.CompanyName);
        }

        [Fact]
        public void ScribeApiConnector_PreConnect_Always_ShouldReturnSerializedFormWithoutPropertyNameDuplicates()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut.PreConnect(new Dictionary<string, string>(0));
            var actualPropertyNames = FormDefinition
                .Deserialize(actual)
                .Entries
                .Select(x => x.PropertyName)
                .ToList();

            Assert.Equal(actualPropertyNames.Distinct().Count(), actualPropertyNames.Count);
        }

        [Fact]
        public void ScribeApiConnector_PreConnect_Always_ShouldReturnSerializedFormWithProperEntries()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut.PreConnect(new Dictionary<string, string>());
            var actualEntries = FormDefinition.Deserialize(actual).Entries;

            Assert.Contains(
                actualEntries,
                x => x.PropertyName == "Environment" && !x.IsRequired && x.InputType == InputType.Text);
            Assert.Contains(
                actualEntries,
                x => x.PropertyName == "Username" && x.IsRequired && x.InputType == InputType.Text);
            Assert.Contains(
                actualEntries,
                x => x.PropertyName == "Password" && x.IsRequired && x.InputType == InputType.Password);
        }

        [Fact]
        public void ScribeApiConnector_PreConnect_Always_ShouldReturnSerializedFormWithOrderedEntries()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut.PreConnect(new Dictionary<string, string>(0));
            var actualForm = FormDefinition.Deserialize(actual);
            var actualProperties = actualForm
                .Entries
                .ToDictionary(x => x.Order, x => x.PropertyName);

            Assert.Equal(actualForm.Entries.Count, actualProperties.Count);
            Assert.Equal("Environment", actualProperties[0]);
            Assert.Equal("Username", actualProperties[1]);
            Assert.Equal("Password", actualProperties[2]);
        }

        [Fact]
        public void ScribeApiConnector_PreConnect_Always_ShouldReturnSerializedFormWithProperlyDefinedEnvironmentOptions()
        {
            var sut = CreateSystemUnderTest();

            var actual = sut.PreConnect(new Dictionary<string, string>(0));
            var actualProperties = FormDefinition
                .Deserialize(actual)
                .Entries
                .Single(x => x.PropertyName == "Environment")
                .Options;

            Assert.All(actualProperties, option =>
            {
                Assert.True(!option.Value.EndsWith("/"));
                Assert.True(!option.Value.EndsWith("\\"));
                Assert.True(Uri.IsWellFormedUriString(option.Value, UriKind.Absolute));
            });
        }

        [Fact]
        public void ScribeApiConnector_Connect_WithValidProperties_ShouldSetIsConnectedToTrue()
        {
            var httpHandler = new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var sut = CreateSystemUnderTest(httpHandler);
            var input = CreateProperties();
            Assert.False(sut.IsConnected);

            sut.Connect(input);

            Assert.True(sut.IsConnected);
        }

        [Fact]
        public void ScribeApiConnector_Connect_WithBadCredentialsInProperties_ShouldThrow()
        {
            var httpHandler = new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            var sut = CreateSystemUnderTest(httpHandler);
            var input = CreateProperties();
            Assert.False(sut.IsConnected);

            var actual = Assert.ThrowsAny<Exception>(() => sut.Connect(input));

            Assert.Contains("Invalid username or password", actual.Message);
            Assert.False(sut.IsConnected);
        }

        [Fact]
        public void ScribeApiConnector_Disconnect_Always_ShouldSetIsConnectedToFalse()
        {
            var sut = CreateSystemUnderTest();
            Assert.False(sut.IsConnected);

            sut.Disconnect();

            Assert.False(sut.IsConnected);
        }

        [Fact]
        public void ScribeApiConnector_Disconnect_AfterSuccessfulConnection_ShouldSetIsConnectedToFalse()
        {
            var sut = CreateSystemUnderTest();
            var input = CreateProperties();
            Assert.False(sut.IsConnected);
            
            sut.Connect(input);
            Assert.True(sut.IsConnected);
            
            sut.Disconnect();
            Assert.False(sut.IsConnected);
        }

        private static ScribeApiConnector CreateSystemUnderTest(HttpMessageHandler handler = null)
        {
            return new ScribeApiConnector(
                handler ?? new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)));
        }

        private static Dictionary<string, string> CreateProperties()
        {
            return new Dictionary<string, string>
            {
                ["Environment"] = "https://sbendpoint.scribesoft.com",
                ["Username"] = "foo",
                ["Password"] = Encryptor.Encrypt_AesManaged(
                    value: "foobar",
                    key: "3103dcf5-6d7c-4b56-8297-f9e449b57576")
            };
        }
    }
}
