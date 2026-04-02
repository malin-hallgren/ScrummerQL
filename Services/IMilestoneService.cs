using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL.Services
{
    internal interface IMilestoneService
    {
        List<Milestone> GetMilestones(GraphQLResponse response);
        Task SaveClosedMilestonesAsync(List<Milestone> milestones);
    }
}
