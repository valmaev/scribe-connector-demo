using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aquiva.Connector.ScribeApi.Domain;
using Aquiva.Connector.ScribeApi.Http;
using Newtonsoft.Json;
using static System.Uri;

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
                });
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

        public static async Task<Organization> GetOrganizationByIdAsync(
            this SingleMediaTypeHttpClient client,
            int organizationId,
            CancellationToken token = default(CancellationToken))
        {
            return await client.SendMediaContentAsync<object, Organization>(
                HttpMethod.Get,
                new Uri($"/v1/orgs/{organizationId}", UriKind.Relative),
                token: token);
        }

        public static async Task<IEnumerable<Organization>> GetOrganizationsAsync(
            this SingleMediaTypeHttpClient client,
            int? parentId = null,
            string name = null,
            string status = null,
            int limit = 100,
            CancellationToken token = default(CancellationToken))
        {
            var query = new StringBuilder("?");

            query.Append(
                parentId != null ? $"parentId={EscapeDataString(parentId.ToString())}&" : "");
            query.Append(
                name != null ? $"name={EscapeDataString(name)}&" : "");
            query.Append(
                status != null ? $"status={EscapeDataString(status)}&" : "");

            var apiUrl = $"/v1/orgs{query}".TrimEnd('&');

            return await Task.FromResult(
                client.GetPaginatedResponse<Organization>(
                    apiUrl,
                    limit,
                    token: token));
        }

        private static IEnumerable<TOutput> GetPaginatedResponse<TOutput>(
            this SingleMediaTypeHttpClient client,
            string apiUrl,
            int limit,
            int offset = 0,
            CancellationToken token = default(CancellationToken))
        {
            bool hasMore;
            do
            {
                var baseUrl = apiUrl.EndsWith("?")
                    ? $"{apiUrl}offset={offset}&limit={limit}"
                    : $"{apiUrl}&offset={offset}&limit={limit}";
                
                var page = client
                    .SendMediaContentAsync<object, IEnumerable<TOutput>>(
                        HttpMethod.Get,
                        new Uri(baseUrl, UriKind.Relative),
                        token: token)
                    .GetAwaiter()
                    .GetResult()
                    ?.ToList() ?? new List<TOutput>(0);

                foreach (var entity in page)
                    yield return entity;

                hasMore = page.Count == limit;
                offset += limit;
            } while (hasMore);
        }
    }
}
