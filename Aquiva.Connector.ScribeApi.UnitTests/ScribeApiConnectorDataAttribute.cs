using System.Net;
using System.Net.Http;
using AutoFixture;
using AutoFixture.Xunit2;

namespace Aquiva.Connector.ScribeApi
{
    public class ScribeApiConnectorDataAttribute : AutoDataAttribute
    {
        private static IFixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Register<HttpMessageHandler>(
                () => new StubbedResponseHandler(
                    new HttpResponseMessage(HttpStatusCode.OK)));
            return fixture;
        }
            
        public ScribeApiConnectorDataAttribute()
            : base(CreateFixture)
        {
        }
    }
}
