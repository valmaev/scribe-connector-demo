using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Aquiva.Connector.ScribeApi
{
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
}
