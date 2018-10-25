using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Aquiva.Connector.ScribeApi.Http;
using Newtonsoft.Json;

namespace Aquiva.Connector.ScribeApi
{
    public static class ScribeApiClient
    {
        public static SingleMediaTypeHttpClient Create(
            Uri baseAddress,
            string username,
            string password)
        {
            return Create(baseAddress, username, password, new HttpClientHandler());
        }

        public static SingleMediaTypeHttpClient Create(
            Uri baseAddress,
            string username,
            string password,
            HttpMessageHandler handler)
        {
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress));
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var httpClient = new HttpClient(handler) {BaseAddress = baseAddress};
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptCharset.Add(
                new StringWithQualityHeaderValue("utf-8"));
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{username}:{password}")));
            return new SingleMediaTypeHttpClient(
                httpClient,
                new JsonMediaTypeFormatter
                {
                    SerializerSettings = new JsonSerializerSettings
                        {NullValueHandling = NullValueHandling.Ignore}
                },
                r => r.EnsureSuccessStatusCode());
        }

        public static async Task CheckConnection(this HttpClient httpClient)
        {
            var testResponse = await httpClient
                .GetAsync(
                    new Uri("/v1/orgs/?limit=1", UriKind.Relative),
                    HttpCompletionOption.ResponseHeadersRead);

            if (testResponse.IsSuccessStatusCode)
                return;

            switch (testResponse.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new InvalidOperationException("Invalid username or password");
                default:
                    throw new InvalidOperationException("Unknown error occured");
            }
        }
    }
}
