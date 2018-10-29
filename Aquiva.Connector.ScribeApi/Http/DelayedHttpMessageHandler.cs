using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aquiva.Connector.ScribeApi.Http
{
    public sealed class DelayedHttpMessageHandler : DelegatingHandler
    {
        private readonly TimeSpan _delayBeforeSend;

        public DelayedHttpMessageHandler(TimeSpan delayBeforeSend)
        {
            _delayBeforeSend = delayBeforeSend;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(_delayBeforeSend, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
