using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Aquiva.Connector.ScribeApi.Http
{
    public class PagedJsonResponseHandler<T> : DelegatingHandler
    {
        private readonly Uri _baseUri;
        private readonly bool _useRelativeUrls;
        private readonly IEnumerable<T> _entities;
        private readonly List<HttpRequestMessage> _requests;

        public PagedJsonResponseHandler(
            IEnumerable<T> entities,
            Uri baseUri,
            bool useRelativeUrls = true)
        {
            _baseUri = baseUri;
            _useRelativeUrls = useRelativeUrls;
            _entities = entities;
            _requests = new List<HttpRequestMessage>();
        }

        public IReadOnlyCollection<HttpRequestMessage> Requests => _requests;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _requests.Add(request);
            var uri = _useRelativeUrls
                ? new Uri(request.RequestUri.LocalPath, UriKind.Relative)
                : new Uri(request.RequestUri.AbsolutePath);

            if (_baseUri != uri)
                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

            var queryParameters = request.RequestUri.ParseQueryString();

            var offset = queryParameters.AllKeys.Contains("offset")
                ? Convert.ToInt32(queryParameters.Get("offset"))
                : 0;
            var limit = queryParameters.AllKeys.Contains("limit")
                ? Convert.ToInt32(queryParameters.Get("limit"))
                : 100;
            return await Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(
                            _entities.Skip(offset).Take(limit)),
                        Encoding.UTF8,
                        "application/json")
                });
        }
    }
}
