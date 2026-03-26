using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL.Services
{
    internal interface IIssueService
    {
        List<Issue> GetIssues(GraphQLResponse response);
    }
}
