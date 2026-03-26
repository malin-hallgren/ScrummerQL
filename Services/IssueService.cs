using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;
using ScrummerQL.Repositories;

namespace ScrummerQL.Services
{
    internal class IssueService : IIssueService
    {
        private readonly IIssueRepository? _repository;

        public IssueService(IIssueRepository? repository = null)
        {
            _repository = repository;
        }
        public List<Issue> GetIssues(GraphQLResponse response)
        {
            List<Issue> issueList = new List<Issue>();
            var nodes = response?.data?.project?.workItems?.nodes ?? new List<WorkItemNode>();

            foreach (var node in nodes)
            {
                if (node.widgets == null)
                    continue;

                if (!int.TryParse(node.iid, out var nodeId))
                    continue;

                var isChild = false;

                foreach (var widget in node.widgets)
                {
                    if (widget?.hasParent == true)
                    {
                        isChild = true;
                        break;
                    }
                }

                if (isChild)
                    continue;

                var currentChildren = new List<ChildIssue>();

                var milestoneId = (int?)null;
                foreach (var widget in node.widgets)
                {
                    if (widget?.milestone?.iid != null && int.TryParse(widget.milestone.iid, out var parsedMilestoneId))
                    {
                        milestoneId = parsedMilestoneId;
                        break;
                    }
                }

                foreach (var widget in node.widgets)
                {
                    if (widget?.children?.nodes == null)
                        continue;

                    foreach (var childNode in widget.children.nodes)
                    {
                        if (childNode == null)
                            continue;

                        if (!int.TryParse(childNode.iid, out var childId))
                            continue;

                        var childIssue = new ChildIssue
                        {
                            GitLabIId = childId,
                            ParentIssueGitLabIId = nodeId,
                            Title = childNode.title,
                            State = childNode.state,
                            Points = 0,
                            Team = "Unassigned",
                            Priority = "Unassigned",
                            Status = "Unassigned"
                        };

                        foreach (var childWidget in childNode.widgets ?? [])
                        {
                            if (childWidget?.labels?.nodes == null)
                                continue;

                            foreach (var childLabel in childWidget.labels.nodes)
                            {
                                if (childLabel?.title == null)
                                    continue;

                                var title = childLabel.title.ToLowerInvariant();

                                if (int.TryParse(title, out var point))
                                {
                                    childIssue.Points = point;
                                }
                                if (title == "prio low" || title == "prio medium" || title == "prio high" || title == "prio critical")
                                {
                                    childIssue.Priority = title.Replace("prio ", "");
                                }
                                if (title == "ui/ux" || title == "backend" || title == "frontend" || title == "devops")
                                {
                                    childIssue.Team = title;
                                }
                                if (title == "active sprint" || title == "in progress" || title == "resolved")
                                {
                                    childIssue.Status = title;
                                }
                            }
                        }

                        currentChildren.Add(childIssue);
                    }
                }


                var issue = new Issue
                {
                    GitLabIId = nodeId,
                    Title = node.title,
                    State = node.state,
                    ChildIssues = currentChildren,
                    inMilestoneWithId = milestoneId
                };

                issueList.Add(issue);
            }

            return issueList;
        }

        public async Task SaveIssuesAsync(List<Issue> issues)
        {
            if (_repository == null)
            {
                throw new InvalidOperationException("Repository is not initialized.");
            }

            foreach (var issue in issues)
            {
                // Prevent cascading save of child issues when adding/updating the parent.
                // Create a shallow copy containing only scalar (DB) properties and save that
                // so EF won't traverse the navigation collection.
                var savedChildren = issue.ChildIssues;
                try
                {
                    var parentOnly = new Issue
                    {
                        Id = issue.Id,
                        GitLabIId = issue.GitLabIId,
                        Title = issue.Title,
                        State = issue.State,
                        inMilestoneWithId = issue.inMilestoneWithId
                    };

                    if (await _repository.ExistsByGitLabIIdAsync(parentOnly.GitLabIId))
                    {
                        if (parentOnly.State.ToLower() != "closed")
                        {
                            await _repository.UpdateAsync(parentOnly);
                        }
                    }
                    else
                    {
                        await _repository.AddAsync(parentOnly);
                        // propagate generated Id back to the original instance
                        issue.Id = parentOnly.Id;
                    }
                }
                finally
                {
                    issue.ChildIssues = savedChildren;
                }
            }
        }

        public async Task SaveChildIssuesAsync(List<Issue> issues)
        {
            if (_repository == null)
            {
                throw new InvalidOperationException("Repository is not initialized.");
            }

            var allChildIssues = new List<ChildIssue>();

            foreach (var issue in issues)
            {
                if (issue.ChildIssues == null || issue.ChildIssues.Count == 0)
                    continue;

                foreach (var childIssue in issue.ChildIssues)
                {
                    var parentIssue = await _repository.GetByGitLabIIdAsync(childIssue.ParentIssueGitLabIId.Value);
                    if (parentIssue != null)
                    {
                        childIssue.ParentIssueId = parentIssue.Id;
                        allChildIssues.Add(childIssue);
                    }
                }
            }

            if (allChildIssues.Count > 0)
            {
                await _repository.SaveChildIssuesAsync(allChildIssues);
            }
        }

    }
}
