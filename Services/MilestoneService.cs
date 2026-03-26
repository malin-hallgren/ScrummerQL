using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL.Services
{
    internal class MilestoneService : IMilestoneService
    {
        public List<Milestone> GetMilestones(GraphQLResponse response)
        {
            List<Milestone> milestoneList = new List<Milestone>();
            var nodes = response?.data?.project?.milestones?.nodes ?? new List<MilestoneNode>();

            foreach (var milestone in nodes)
            {
                if (!int.TryParse(milestone.iid, out var milestoneId) || string.IsNullOrWhiteSpace(milestone.startDate))
                    continue;

                var newMilestone = new Milestone
                {
                    Id = milestoneId,
                    Title = milestone.title,
                    StartDate = DateOnly.Parse(milestone.startDate),
                    EndDate = string.IsNullOrWhiteSpace(milestone.dueDate)
                        ? null
                        : DateOnly.Parse(milestone.dueDate),
                    Issues = new List<Issue>(),
                    TotalPoints = 0,
                    CompletedPoints = 0
                };

                milestoneList.Add(newMilestone);
            }

            return milestoneList;
        }
    }
}
