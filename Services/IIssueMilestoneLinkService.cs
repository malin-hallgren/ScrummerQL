using System;
using System.Collections.Generic;
using System.Text;
using ScrummerQL.Model;

namespace ScrummerQL.Services
{
    internal interface IIssueMilestoneLinkService
    {
        void LinkIssuesToMilestones(List<Issue> issues, List<Milestone> milestones);
    }
}
