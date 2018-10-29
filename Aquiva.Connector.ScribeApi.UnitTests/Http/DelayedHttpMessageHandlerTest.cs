using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Aquiva.Connector.ScribeApi.Http
{
    public class DelayedHttpMessageHandlerTest
    {
        [Fact]
        public async Task DelayedHttpMessageHandler_SendAsync_Always_ShouldDelayRequest()
        {
            // Arrange
            var expected = TimeSpan.FromSeconds(1);
            var client = new HttpClient(
                new DelayedHttpMessageHandler(expected)
                {
                    InnerHandler = new StubbedResponseHandler(
                        new HttpResponseMessage(HttpStatusCode.OK))
                });

            // Act
            var actual = new Stopwatch();
            actual.Start();
            await client.GetAsync("http://foobar");
            actual.Stop();

            // Assert
            var diff = actual.Elapsed - expected;
            Assert.True(expected <= actual.Elapsed);
            Assert.True(diff >= TimeSpan.Zero);
        }
    }
}
