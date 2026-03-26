using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ScrummerQL.ResponseHelpers
{
    internal class QLResponseHandler
    {
        private HttpClient _httpClient;

        public QLResponseHandler(HttpClient client)
        {
            _httpClient = client;
        }
        public async Task<string> GetResponseAsync(string query, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Add("GraphQL-Features", "sub_issues");

            var body = new { query = query };

            var response = await _httpClient.PostAsJsonAsync(
                "https://git.chas-lab.dev/api/graphql",
                body
            );

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"GraphQL request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}). Response: {json}"
                );
            }

            return json;
        }
    }
}
