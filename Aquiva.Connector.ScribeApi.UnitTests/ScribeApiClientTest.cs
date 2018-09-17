using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Xunit;

namespace Aquiva.Connector.ScribeApi
{
    public class ScribeApiClientTest
    {
        [Theory, ConnectorData]
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
            Assert.Equal(baseAddress, actual.BaseAddress);
            Assert.Equal("Basic", actual.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal(expectedAuthParam, actual.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Contains(actual.DefaultRequestHeaders.Accept, x => x.MediaType == "application/json");
            Assert.Contains(actual.DefaultRequestHeaders.AcceptCharset, x => x.Value == "utf-8");
        }

        [Theory, MemberData(nameof(SuccessfulHttpStatusCodes))]
        public async Task ScribeApiClient_CheckConnection_OnSuccessfulStatusCode_ShouldNotThrow(
            HttpStatusCode successfulStatusCode)
        {
            var handler = new StubbedResponseHandler(new HttpResponseMessage(successfulStatusCode));
            var sut = ScribeApiClient.Create(new Uri("https://foo"), "bar", "baz", handler);

            var actual = await Record.ExceptionAsync(() => sut.CheckConnection());

            Assert.Null(actual);
        }

        [Fact]
        public async Task ScribeApiClient_CheckConnection_OnUnauthorized_ShouldThrow()
        {
            var handler = new StubbedResponseHandler(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            var sut = ScribeApiClient.Create(new Uri("https://foo"), "bar", "baz", handler);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CheckConnection());

            Assert.Contains("Invalid username or password", actual.Message);
        }

        [Theory, MemberData(nameof(FailureHttpStatusCodesWithoutSpecialHandling))]
        public async Task ScribeApiClient_CheckConnection_OnOtherFailureStatusCodes_ShouldThrow(
            HttpStatusCode failureStatusCode)
        {
            var handler = new StubbedResponseHandler(new HttpResponseMessage(failureStatusCode));
            var sut = ScribeApiClient.Create(new Uri("https://foo"), "bar", "baz", handler);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CheckConnection());

            Assert.Contains("Unknown error occured", actual.Message);
        }

        public static IEnumerable<object[]> SuccessfulHttpStatusCodes()
        {
            return Enum.GetValues(typeof(HttpStatusCode))
                .Cast<int>()
                .Where(c => c >= 200 && c <= 299)
                .Distinct()
                .Select(c => new object[] {c});
        }

        public static IEnumerable<object[]> FailureHttpStatusCodesWithoutSpecialHandling()
        {
            return Enum.GetValues(typeof(HttpStatusCode))
                .Cast<int>()
                .Where(c => (c < 200 || c > 299) && c != 401)
                .Distinct()
                .Select(c => new object[] {c});
        }

        public class StubbedResponseHandler : DelegatingHandler
        {
            private readonly HttpResponseMessage _response;
            private readonly List<HttpRequestMessage> _requests;

            public StubbedResponseHandler(HttpResponseMessage response)
            {
                _response = response;
                _requests = new List<HttpRequestMessage>();
            }

            public IReadOnlyCollection<HttpRequestMessage> Requests => _requests;

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                _requests.Add(request);
                return await Task.FromResult(_response);
            }
        }

        public class ConnectorDataAttribute : AutoDataAttribute
        {
            private static IFixture CreateFixture()
            {
                var fixture = new Fixture();
                fixture.Register<HttpMessageHandler>(
                    () => new StubbedResponseHandler(
                        new HttpResponseMessage(HttpStatusCode.OK)));
                return fixture;
            }
            
            public ConnectorDataAttribute()
                : base(CreateFixture)
            {
            }
        }
    }
}
