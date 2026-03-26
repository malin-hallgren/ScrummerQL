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
        public Milestones? milestones { get; set; }
        public WorkItems? workItems { get; set; }
    }

    public class Milestones
    {
        public List<MilestoneNode>? nodes { get; set; }
    }

    public class MilestoneNode
    {
        public string? iid { get; set; }
        public string? title { get; set; }
        public string? state { get; set; }

        public string? startDate { get; set; }
        public string? dueDate { get; set; }

    }

    public class WorkItems
    {
        public List<WorkItemNode>? nodes { get; set; }
    }

    public class WorkItemNode
    {
        public string? iid { get; set; }
        public string? title { get; set; }
        public string? state { get; set; }
        
        public List<NodeWidget?> widgets { get; set; }
    }

    public class NodeWidget
    {
        public MilestoneNode? milestone { get; set; }
        public bool? hasParent { get; set; }

        public Children? children { get; set; }

    }

    public class MilestoneWidget
    {
        public string? iid { get; set; }
        public string? title { get; set; }
    }

    public class Children
    {
        public List<ChildIssueNode> nodes { get; set; }
    }

    public class ChildIssueNode
    {
        public string? iid { get; set; }
        public string? title { get; set; }
        public string? state { get; set; }
        
        public List<ChildIssueWidget> widgets { get; set; }
    }

    public class ChildIssueWidget
    {
        public Labels labels { get; set; }
    }

    public class Labels
    {
        public List<LabelNode?> nodes { get; set; }
    }

    public class LabelNode
    {
        public string? title { get; set; }
    }

    public class GraphQLError
    {
        public string? message { get; set; }
    }
}
