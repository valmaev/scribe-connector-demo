using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Aquiva.Connector.ScribeApi.Domain;
using Aquiva.Connector.ScribeApi.Http;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Xunit;

namespace Aquiva.Connector.ScribeApi
{
    public class ScribeApiClientTest
    {
        [Theory, ScribeApiConnectorData]
        public void ScribeApiClient_AllSyncPublicMembers_Always_ShouldHaveNullGuards(
            GuardClauseAssertion assertion)
        {
            // Filtering out async methods from guard clause testing since they're lazily
            // evaluated, therefore they don't conform Fail Fast principle
            var sut = typeof(ScribeApiClient)
                .GetMethods()
                .Where(x => x.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) == null);
            assertion.Verify(sut);
        }

        [Theory, AutoData]
        public void ScribeApiClient_Create_Always_ShouldConstructProperlyConfiguredHttpClient(
            Uri baseAddress,
            string username,
            string password)
        {
            var actual = ScribeApiClient.Create(baseAddress, username, password);

            var expectedAuthParam = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{username}:{password}"));
            Assert.Equal(baseAddress, actual.HttpClient.BaseAddress);
            Assert.Equal("Basic", actual.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal(expectedAuthParam, actual.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Contains(actual.HttpClient.DefaultRequestHeaders.Accept, x => x.MediaType == "application/json");
            Assert.Contains(actual.HttpClient.DefaultRequestHeaders.AcceptCharset, x => x.Value == "utf-8");
        }

        [Theory, MemberData(nameof(HttpStatusCodesIndicatingSuccess))]
        public async Task ScribeApiClient_CheckConnection_OnSuccessfulStatusCode_ShouldNotThrow(
            HttpStatusCode successfulStatusCode)
        {
            var handler = new StubbedResponseHandler(new HttpResponseMessage(successfulStatusCode));
            var sut = ScribeApiClient.Create(new Uri("https://foo"), "bar", "baz", handler);

            var actual = await Record.ExceptionAsync(() => sut.HttpClient.CheckConnection());

            Assert.Null(actual);
        }

        [Fact]
        public async Task ScribeApiClient_CheckConnection_OnUnauthorized_ShouldThrow()
        {
            var handler = new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            var sut = ScribeApiClient.Create(new Uri("https://foo"), "bar", "baz", handler);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.HttpClient.CheckConnection());

            Assert.Contains("Invalid username or password", actual.Message);
        }

        [Theory, MemberData(nameof(HttpStatusCodesIndicatingFailureWithoutSpecialHandling))]
        public async Task ScribeApiClient_CheckConnection_OnOtherFailureStatusCodes_ShouldThrow(
            HttpStatusCode failureStatusCode)
        {
            var handler = new StubbedResponseHandler(new HttpResponseMessage(failureStatusCode));
            var sut = ScribeApiClient.Create(new Uri("https://foo"), "bar", "baz", handler);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.HttpClient.CheckConnection());

            Assert.Contains("Unknown error occured", actual.Message);
        }

        [Theory, AutoData]
        public async Task ScribeApiClient_GetOrganizationByIdAsync_Always_ShouldSendRequestProperly(
            int organizationId)
        {
            var stubHandler = new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var sut = CreateSystemUnderTest(handler: stubHandler);

            await sut.GetOrganizationByIdAsync(organizationId);

            var actual = stubHandler.Requests.Single();
            Assert.Equal(HttpMethod.Get, actual.Method);
            Assert.Equal($"/v1/orgs/{organizationId}", actual.RequestUri.LocalPath);
        }

        [Theory]
        [InlineData("offset=0&limit=100", null, null, null)]
        [InlineData("status=foo&offset=0&limit=100", null, null, "foo")]
        [InlineData("name=bar&offset=0&limit=100", null, "bar", null)]
        [InlineData("parentId=1&offset=0&limit=100", 1, null, null)]
        [InlineData("parentId=1&name=baz&offset=0&limit=100", 1, "baz", null)]
        [InlineData("name=qux&status=quux&offset=0&limit=100", null, "qux", "quux")]
        [InlineData("parentId=1&status=corge&offset=0&limit=100", 1, null, "corge")]
        [InlineData("parentId=1&name=&status=&offset=0&limit=100", 1, "", "")]
        [InlineData("parentId=1&name=grault&status=&offset=0&limit=100", 1, "grault", "")]
        [InlineData("parentId=1&name=&status=garply&offset=0&limit=100", 1, "", "garply")]
        public async Task ScribeApiClient_GetOrganizationsAsync_Always_ShouldSendRequestProperly(
            string expectedQuery,
            int? parentId,
            string name,
            string status)
        {
            // Arrange
            var expected = $"/v1/orgs?{expectedQuery}";

            var stubHandler = new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var sut = CreateSystemUnderTest(handler: stubHandler);

            // Act
            (await sut.GetOrganizationsAsync(parentId, name, status)).ToList();

            // Assert
            var actual = stubHandler.Requests.Single();
            Assert.Equal(HttpMethod.Get, actual.Method);
            Assert.Equal(expected, actual.RequestUri.PathAndQuery);
        }
        
        [Theory]
        [InlineData(1, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(1, 100, 50)]
        [InlineData(1, 100, 100)]
        [InlineData(100, 100, 100)]
        public async Task ScribeApiClient_GetOrganizationsAsync_Always_ShouldLazilySendExpectedNumberOfRequests(
            int limit,
            int totalNumberOfEntities,
            int numberOfEntitiesToTake)
        {
            // Arrange
            var responseHandler = new PagedJsonResponseHandler<Organization>(
                new Fixture().CreateMany<Organization>(totalNumberOfEntities).ToList(), 
                new Uri("/v1/orgs", UriKind.Relative));
            var sut = CreateSystemUnderTest(
                handler: responseHandler, 
                formatter: new JsonMediaTypeFormatter());

            // Act
            var actual = (await sut
                .GetOrganizationsAsync(limit: limit))
                .Take(numberOfEntitiesToTake)
                .ToList();

            // Assert
            int expected = numberOfEntitiesToTake / limit;
            Assert.Equal(expected, responseHandler.Requests.Count);
            Assert.Equal(numberOfEntitiesToTake, actual.Count);
        }

        public static IEnumerable<object[]> HttpStatusCodesIndicatingSuccess()
        {
            return Enum.GetValues(typeof(HttpStatusCode))
                .Cast<int>()
                .Where(c => c >= 200 && c <= 299)
                .Distinct()
                .Select(c => new object[] {c});
        }

        public static IEnumerable<object[]> HttpStatusCodesIndicatingFailureWithoutSpecialHandling()
        {
            return Enum.GetValues(typeof(HttpStatusCode))
                .Cast<int>()
                .Where(c => (c < 200 || c > 299) && c != 401)
                .Distinct()
                .Select(c => new object[] {c});
        }
        
        private static SingleMediaTypeHttpClient CreateSystemUnderTest(
            HttpMessageHandler handler = null,
            MediaTypeFormatter formatter = null)
        {
            return new SingleMediaTypeHttpClient(
                new HttpClient(
                        handler ?? new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.OK)))
                    {BaseAddress = new Uri("http://foobar")},
                formatter ?? new DummyMediaTypeFormatter());
        }
    }
}
