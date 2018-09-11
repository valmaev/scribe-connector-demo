using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Aquiva.Connector.ScribeApi
{
    public static class ScribeApiClient
    {
        public static HttpClient Create(
            Uri baseAddress,
            string username,
            string password)
        {
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress));
            if (username == null)
                throw new ArgumentNullException(nameof(username));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var httpClient = new HttpClient {BaseAddress = baseAddress};
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.AcceptCharset.Add(
                new StringWithQualityHeaderValue("utf-8"));
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{username}:{password}")));
            return httpClient;
        }
    }
}
