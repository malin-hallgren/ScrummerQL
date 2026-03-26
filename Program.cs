using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using ScrummerQL.Model;
using ScrummerQL.Repositories;

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
              project(fullPath: ""chas-challenge-2026/grupp-10/grupp-10-cc-2026"") {
                milestones(first: 10) {
                  nodes {
                    iid
                    title
                    state
                    startDate
                    dueDate
                  }
                }
                workItems(first: 100) {
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
            var repo = new ResponseRepository(client);

            var json = await repo.GetResponseAsync(query);


            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("Empty response returned from GraphQL.");
                return;
            }

            GraphQLResponse? graphQlResponse;

            try
            {
                graphQlResponse = JsonSerializer.Deserialize<GraphQLResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to deserialize GraphQL response: {ex.Message}");
                Console.WriteLine(json);
                return;
            }

            if (graphQlResponse?.errors != null)
            {
                foreach (var error in graphQlResponse.errors)
                    Console.WriteLine(error.message);

                return;
            }

            var (milestoneList, issueList) = Parser.Parse(graphQlResponse!);

            Printer.PrintByMilestone(milestoneList, issueList);
            //Printer.PrintIssuesWithoutMilestone(issueList.Where(x => x.inMilestoneWithId == null).ToList());

            //foreach (var workItem in workItems.EnumerateArray())
            //{
            //    var workItemId = int.Parse(workItem.GetProperty("iid").GetString()!);

            //    var hasParent = false;
            //    if (workItem.TryGetProperty("widgets", out var widgets))
            //    {
            //        foreach (var widget in widgets.EnumerateArray())
            //        {
            //            if (widget.TryGetProperty("hasParent", out var hp) && hp.GetBoolean())
            //            {
            //                hasParent = true;
            //                break;
            //            }
            //        }
            //    }

            //    if (hasParent)
            //        continue;

            //    var newIssue = new Issue
            //    {
            //        Id = workItemId,
            //        Title = workItem.GetProperty("title").GetString(),
            //        State = workItem.GetProperty("state").GetString(),
            //        ChildIssues = new List<ChildIssue>(),
            //        inMilestoneWithId = null
            //    };
            //    if (workItem.TryGetProperty("widgets", out var widg))
            //    {
            //        Milestone? issueMilestone = null;
            //        JsonElement childrenConnection = default;
            //        var hasChildren = false;

            //        foreach (var widget in widg.EnumerateArray())
            //        {
            //            if(widget.TryGetProperty("milestone", out var milestone) && milestone.ValueKind == JsonValueKind.Object)
            //            {
            //                var milestoneId = int.Parse(milestone.GetProperty("iid").GetString());
            //                issueMilestone = milestoneList.FirstOrDefault(m => m.Id == milestoneId);
            //            }

            //            if (widget.TryGetProperty("children", out var children))
            //            {
            //                childrenConnection = children;
            //                hasChildren = true;
            //            }
            //        }

            //        if (issueMilestone != null)
            //        {
            //            issueMilestone.Issues.Add(newIssue);
            //            newIssue.inMilestoneWithId = issueMilestone.Id;
            //        }

            //        if (!hasChildren)
            //            continue;

            //        var childNodes = childrenConnection.GetProperty("nodes").EnumerateArray();

            //        foreach (var child in childNodes)
            //        {
            //            var childIssue = new ChildIssue
            //            {
            //                Id = int.Parse(child.GetProperty("iid").GetString()!),
            //                Title = child.GetProperty("title").GetString()!,
            //                Points = 0,
            //                Team = "",
            //                Priority = "",
            //                Status = "",
            //                State = child.GetProperty("state").GetString()!,
            //                ParentIssueId = newIssue.Id
            //            };

            //            if (!child.TryGetProperty("widgets", out var childWidgets))
            //                continue;

            //            foreach (var innerwidget in childWidgets.EnumerateArray())
            //            {
            //                if (!innerwidget.TryGetProperty("labels", out var labelsWidget) ||
            //                    !labelsWidget.TryGetProperty("nodes", out var labelNodes))
            //                {
            //                    continue;
            //                }

            //                foreach (var label in labelNodes.EnumerateArray())
            //                {
            //                    var title = label.GetProperty("title").GetString()?.ToLowerInvariant();

            //                    if (string.IsNullOrWhiteSpace(title))
            //                    {
            //                        continue;
            //                    }

            //                    if (int.TryParse(title, out var point))
            //                    {
            //                        childIssue.Points = point;
            //                    }
            //                    if (title == "prio low" || title == "prio medium" || title == "prio high" || title == "prio critical")
            //                    {
            //                        childIssue.Priority = title.Replace("prio ", "");
            //                    }
            //                    if (title == "ui/ux" || title == "backend" || title == "frontend" || title == "devops")
            //                    {
            //                        childIssue.Team = title;
            //                    }
            //                    if (title == "active sprint" || title == "in progress" || title == "resolved")
            //                    {
            //                        childIssue.Status = title;
            //                    }
            //                }
            //            }
            //            newIssue.ChildIssues.Add(childIssue);

            //            if (issueMilestone != null)
            //            {
            //                issueMilestone.TotalPoints += childIssue.Points;

            //                if (string.Equals(childIssue.State, "closed", StringComparison.OrdinalIgnoreCase))
            //                    issueMilestone.CompletedPoints += childIssue.Points;
            //            }

            //        }
            //    }

            //    allIssues.Add(newIssue);
            //}

            //var childIssueIds = allIssues
            //    .SelectMany(x => x.ChildIssues)
            //    .Select(x => x.Id)
            //    .ToHashSet();

            //Printer.PrintByMilestone(milestoneList);
            //Printer.PrintIssuesWithoutMilestone(
            //    allIssues.Where(x => x.inMilestoneWithId == null && !childIssueIds.Contains(x.Id)).ToList()
            //);
        }
    }
}
