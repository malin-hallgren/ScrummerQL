using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL.Services
{
    internal class IssueMilestoneLinkService : IIssueMilestoneLinkService
    {
        public void LinkIssuesToMilestones(List<Issue> issues, List<Milestone> milestones)
        {
            foreach (var milestone in milestones)
            {
                milestone.Issues = issues.Where(i => i.inMilestoneWithId == milestone.Id).ToList();
                milestone.TotalPoints = milestone.Issues.Sum(i => i.ChildIssues.Sum(ci => ci.Points));

                foreach (var issue in milestone.Issues)
                {
                    foreach (var childIssue in issue.ChildIssues)
                    {
                        if (childIssue.State.ToLower() == "closed")
                        {
                            milestone.CompletedPoints += childIssue.Points;
                        }
                    }
                    
                }
            }
        }
    }
}
