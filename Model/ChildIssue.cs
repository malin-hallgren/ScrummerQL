using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL.Model
{
    internal class ChildIssue
    {
        public int Id { get; set; }
        public int GitLabIId { get; set; }
        public int ParentIssueId { get; set; }

        public int? ParentIssueGitLabIId { get; set; }   
        public string Title { get; set; }
        public string State { get; set; }
        public int Points { get; set; }
        public string Team { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
    }
}
