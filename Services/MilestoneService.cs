using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;
using ScrummerQL.Repositories;

namespace ScrummerQL.Services
{
    internal class MilestoneService : IMilestoneService
    {
        private readonly IMilestoneRepository? _repository;

        public MilestoneService(IMilestoneRepository? repository = null)
        {
            _repository = repository;
        }

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
                    GitLabIId = milestoneId,
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

        public async Task SaveClosedMilestonesAsync(List<Milestone> milestones)
        {
            if (_repository == null)
                throw new InvalidOperationException("Repository is not initialized.");

            foreach (var milestone in milestones)
            {
                if (milestone.EndDate.HasValue && milestone.EndDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    if (!await _repository.ExistsByGitLabIIdAsync(milestone.GitLabIId))
                    {
                        await _repository.AddAsync(milestone);
                    }
                }
            }
        }
    }
}
