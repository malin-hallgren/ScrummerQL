using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using ScrummerQL.Model;

namespace ScrummerQL
{
    internal class Program
    {
        //malin-hallgren-chas/testteam10
        //chas-challenge-2026/grupp-10/grupp-10-cc-2026
        static async Task Main(string[] args)
        {
            string query = @"
            query {
              project(fullPath: ""malin-hallgren-chas/testteam10"") {
                milestones(first: 10) {
                  nodes {
                    iid
                    title
                    startDate
                    dueDate
                  }
                }
                workItems(first: 50) {
                  nodes {
                    iid
                    title
                    state
                    widgets {
                      ... on WorkItemWidgetMilestone {
                        milestone {
                          iid
                          title
                        }
                      }
                      ... on WorkItemWidgetHierarchy {
                        hasParent
                        children(first: 50) {
                          nodes {
                            iid
                            title
                            state
                            widgets {
                              ... on WorkItemWidgetLabels {
                                labels {
                                  nodes {
                                    title
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", File.ReadAllText("../../../.env"));
            client.DefaultRequestHeaders.Add("GraphQL-Features", "sub_issues");

            var body = new { query = query };

            var response = await client.PostAsJsonAsync(
                "https://git.chas-lab.dev/api/graphql",
                body
            );

            var json = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(json);

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                foreach (var error in errors.EnumerateArray())
                    Console.WriteLine(error.GetProperty("message").GetString());

                return;
            }

            var workItems = doc.RootElement
                .GetProperty("data")
                .GetProperty("project")
                .GetProperty("workItems")
                .GetProperty("nodes");

            var milestones = doc.RootElement
                .GetProperty("data")
                .GetProperty("project")
                .GetProperty("milestones")
                .GetProperty("nodes");

            List<Milestone> milestoneList = new List<Milestone>();

            foreach(var milestone in milestones.EnumerateArray())
            {
                var startDateElement = milestone.GetProperty("startDate");
                if (startDateElement.ValueKind == JsonValueKind.Null)
                    continue; // or handle it another way

                var newMilestone = new Milestone
                {
                    Id = int.Parse(milestone.GetProperty("iid").GetString()!),
                    Title = milestone.GetProperty("title").GetString()!,
                    StartDate = DateOnly.Parse(startDateElement.GetString()!),
                    EndDate = milestone.GetProperty("dueDate").ValueKind == JsonValueKind.Null
                        ? null
                        : DateOnly.Parse(milestone.GetProperty("dueDate").GetString()!),
                    Issues = new List<Issue>(),
                    TotalPoints = 0,
                    CompletedPoints = 0
                };
                milestoneList.Add(newMilestone);
            }


            foreach (var workItem in workItems.EnumerateArray())
            {
                var widgets = workItem.GetProperty("widgets");

                var hasParent = false;
                foreach (var widget in widgets.EnumerateArray())
                {
                    if (widget.TryGetProperty("hasParent", out var hp) && hp.GetBoolean())
                    {
                        hasParent = true;
                        break;
                    }
                }

                if (hasParent)
                    continue;

                var newIssue = new Issue
                {
                    Id = int.Parse(workItem.GetProperty("iid").GetString()),
                    Title = workItem.GetProperty("title").GetString(),
                    State = workItem.GetProperty("state").GetString(),
                    ChildIssues = new List<ChildIssue>()
                };
                if (workItem.TryGetProperty("widgets", out var widg))
                {
                    Milestone? issueMilestone = null;
                    JsonElement childrenConnection = default;
                    var hasChildren = false;

                    foreach (var widget in widg.EnumerateArray())
                    {
                        if(widget.TryGetProperty("milestone", out var milestone) && milestone.ValueKind == JsonValueKind.Object)
                        {
                            var milestoneId = int.Parse(milestone.GetProperty("iid").GetString());
                            issueMilestone = milestoneList.FirstOrDefault(m => m.Id == milestoneId);
                        }

                        if (widget.TryGetProperty("children", out var children))
                        {
                            childrenConnection = children;
                            hasChildren = true;
                        }
                    }

                    if (issueMilestone != null)
                    {
                        issueMilestone.Issues.Add(newIssue);
                    }

                    if (!hasChildren)
                        continue;

                    var childNodes = childrenConnection.GetProperty("nodes").EnumerateArray();

                    foreach (var child in childNodes)
                    {
                        var childIssue = new ChildIssue
                        {
                            Id = int.Parse(child.GetProperty("iid").GetString()!),
                            Title = child.GetProperty("title").GetString()!,
                            Points = 0,
                            Team = "",
                            Priority = "",
                            Status = "",
                            State = child.GetProperty("state").GetString()!
                        };

                        if (!child.TryGetProperty("widgets", out var childWidgets))
                            continue;

                        foreach (var innerwidget in childWidgets.EnumerateArray())
                        {
                            if (!innerwidget.TryGetProperty("labels", out var labelsWidget) ||
                                !labelsWidget.TryGetProperty("nodes", out var labelNodes))
                            {
                                continue;
                            }

                            foreach (var label in labelNodes.EnumerateArray())
                            {
                                var title = label.GetProperty("title").GetString()?.ToLowerInvariant();

                                if (string.IsNullOrWhiteSpace(title))
                                {
                                    continue;
                                }

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

                        newIssue.ChildIssues.Add(childIssue);

                        if (issueMilestone != null)
                        {
                            issueMilestone.TotalPoints += childIssue.Points;

                            if (string.Equals(childIssue.State, "closed", StringComparison.OrdinalIgnoreCase))
                                issueMilestone.CompletedPoints += childIssue.Points;
                        }

                    }
                }
            }

            Printer.PrintByMilestone(milestoneList);
        }
    }
}
