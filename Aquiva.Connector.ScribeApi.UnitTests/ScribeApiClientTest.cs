using System;
using System.Text;
using AutoFixture.Idioms;
using AutoFixture.Xunit2;
using Xunit;

namespace Aquiva.Connector.ScribeApi
{
    public class ScribeApiClientTest
    {
        [Theory, AutoData]
        public void ScribeApiClient_AllPublicMembers_Always_ShouldHaveNullGuards(
            GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(ScribeApiClient));
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
    }
}
