using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
    }
}
