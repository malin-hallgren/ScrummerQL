using System.Collections.Generic;

namespace ScrummerQL
{
    public class GraphQLResponse
    {
        public Data? data { get; set; }
        public List<GraphQLError>? errors { get; set; }
    }

    public class Data
    {
        public Project? project { get; set; }
    }

    public class Project
    {
        public Issues? issues { get; set; }
    }

    public class Issues
    {
        public List<IssueNode>? nodes { get; set; }
    }

    public class IssueNode
    {
        public string? iid { get; set; }
        public string? title { get; set; }
        public string? state { get; set; }
        public string? description { get; set; }
        public Labels? labels { get; set; }
        public SubIssues? subIssues { get; set; }
    }

    public class Labels
    {
        public List<LabelNode>? nodes { get; set; }
    }

    public class LabelNode
    {
        public string? title { get; set; }
    }

    public class SubIssues
    {
        public List<IssueNode>? nodes { get; set; }
    }

    public class GraphQLError
    {
        public string? message { get; set; }
    }
}
