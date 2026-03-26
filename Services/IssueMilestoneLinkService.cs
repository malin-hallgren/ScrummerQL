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
            }
        }
    }
}
