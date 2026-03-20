using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL.Model
{
    internal class Issue
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string State { get; set; }

        public List<ChildIssue> ChildIssues { get; set; }

        public int? inMilestoneWithId { get; set; }
    }
}
