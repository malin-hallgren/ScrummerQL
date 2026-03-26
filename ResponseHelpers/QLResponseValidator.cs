using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ScrummerQL.ResponseHelpers
{
    internal class QLResponseValidator
    {
        public static GraphQLResponse? ValidateResponse(string json)
        {
            GraphQLResponse? graphQlResponse;

            try
            {
                graphQlResponse = JsonSerializer.Deserialize<GraphQLResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                return graphQlResponse;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to deserialize GraphQL response: {ex.Message}");
                return null;
            }
        }
    }
}
