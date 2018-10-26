using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Aquiva.Connector.ScribeApi.Http
{
    public sealed class SingleMediaTypeHttpClient : IDisposable
    {
        private static readonly Action<HttpResponseMessage> DefaultResponseAnalyzer =
            r => r.EnsureSuccessStatusCode();

        public SingleMediaTypeHttpClient(
            HttpClient httpClient,
            MediaTypeFormatter formatter)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            HttpClient = httpClient;
            Formatter = formatter;
        }

        public HttpClient HttpClient { get; }
        public MediaTypeFormatter Formatter { get; }
        public Action<HttpResponseMessage> ResponseAnalyzer { get; set; } = DefaultResponseAnalyzer;

        public async Task<TResp> SendMediaContentAsync<TReq, TResp>(
            HttpMethod httpMethod,
            Uri uri,
            TReq requestBody = default(TReq),
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
            CancellationToken token = default(CancellationToken))
        {
            var content = httpMethod == HttpMethod.Get
                ? null
                : new ObjectContent<TReq>(requestBody, Formatter);
            var request = new HttpRequestMessage(httpMethod, uri) {Content = content};
            var response = await HttpClient.SendAsync(request, completionOption, token);
            ResponseAnalyzer?.Invoke(response);
            return response.StatusCode == HttpStatusCode.NoContent || response.Content == null
                ? default(TResp)
                : await response.Content.ReadAsAsync<TResp>(
                    new[] {Formatter},
                    token);
        }

        public void Dispose() => HttpClient?.Dispose();
    }
}
