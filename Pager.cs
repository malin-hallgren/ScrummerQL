using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL
{
    internal class Pager
    {
        public static bool ExtractHasNextPage(GraphQLResponse? response)
        {
            return response?.data?.project?.workItems?.pageInfo?.hasNextPage ?? false;
        }

        public static string? ExtractEndCursor(GraphQLResponse? response)
        {
            return response?.data?.project?.workItems?.pageInfo?.endCursor;
        }
    }
}
