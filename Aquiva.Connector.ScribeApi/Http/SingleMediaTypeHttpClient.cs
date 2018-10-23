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
        private readonly Action<HttpResponseMessage> _responseAnalyzer;

        public SingleMediaTypeHttpClient(
            HttpClient httpClient,
            MediaTypeFormatter formatter,
            Action<HttpResponseMessage> responseAnalyzer)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));
            if (responseAnalyzer == null)
                throw new ArgumentNullException(nameof(responseAnalyzer));

            HttpClient = httpClient;
            Formatter = formatter;
            _responseAnalyzer = responseAnalyzer;
        }

        public HttpClient HttpClient { get; }
        public MediaTypeFormatter Formatter { get; }

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
            _responseAnalyzer(response);
            return response.StatusCode == HttpStatusCode.NoContent || response.Content == null
                ? default(TResp)
                : await response.Content.ReadAsAsync<TResp>(
                    new[] {Formatter},
                    token);
        }

        public void Dispose() => HttpClient?.Dispose();
    }
}
