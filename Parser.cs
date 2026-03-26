using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace ScrummerQL
{
    internal class Parser
    {
        public static List<Issue> ParseWorkItems(GraphQLResponse response)
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

                foreach (var widget in node.widgets) {
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
                            Id = childId,
                            ParentIssueId = nodeId,
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
                    Id = nodeId,
                    Title = node.title,
                    State = node.state,
                    ChildIssues = currentChildren,
                    inMilestoneWithId = milestoneId
                };

                issueList.Add(issue);
            }

            return issueList;
        }

        public static List<Milestone> ParseMilestones(GraphQLResponse response)
        {
            List<Milestone> milestoneList = new List<Milestone>();
            var nodes = response?.data?.project?.milestones?.nodes ?? new List<MilestoneNode>();
            List<Issue> issueList = ParseWorkItems(response);

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
                    Issues = issueList.Where(i => i.inMilestoneWithId == milestoneId).ToList(),
                    TotalPoints = 0,
                    CompletedPoints = 0
                };

                milestoneList.Add(newMilestone);
            }

            return milestoneList;
        }

        public static (List<Milestone> Milestones, List<Issue> Issues) Parse(GraphQLResponse data)
        {
            return (ParseMilestones(data), ParseWorkItems(data)); // milestones need to take issues as params to calculate points
        }
    }
}
