using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace ScrummerQL
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string query = @"
            query {
              project(fullPath: ""chas-challenge-2026/grupp-10/grupp-10-cc-2026"") {
                milestones(first: 10) {
                  nodes {
                    iid
                    title
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

            foreach(var milestone in milestones.EnumerateArray())
            {  Console.WriteLine(milestone.GetProperty("title").GetString()); }

            foreach (var item in workItems.EnumerateArray())
            {
                if (!item.TryGetProperty("widgets", out var widgets))
                    continue;

                var hasParent = false;
                foreach (var widget in widgets.EnumerateArray())
                {
                    if (widget.TryGetProperty("hasParent", out var hp))
                    {
                        hasParent = hp.GetBoolean();
                        break;
                    }
                }

                if (hasParent)
                    continue;

                Console.WriteLine($"Work item: {item.GetProperty("title").GetString()}");

                foreach (var widget in widgets.EnumerateArray())
                {
                    if(widget.TryGetProperty("milestone", out var milestone) && milestone.ValueKind == JsonValueKind.Object)
                    {
                        Console.WriteLine($"  Milestone: {milestone.GetProperty("title").GetString()}");
                    }
                    if (!widget.TryGetProperty("children", out var childrenConnection))
                        continue;

                    foreach (var child in childrenConnection.GetProperty("nodes").EnumerateArray())
                    {

                        Console.WriteLine($"  Child: {child.GetProperty("title").GetString()}");

                        if (!child.TryGetProperty("widgets", out var childWidgets))
                            continue;

                        foreach (var innerwidget in childWidgets.EnumerateArray())
                        {
                            if (!innerwidget.TryGetProperty("labels", out var labelsWidget) ||
                                !labelsWidget.TryGetProperty("nodes", out var labelNodes))
                            {
                                continue;
                            }

                            Console.WriteLine("\tLabels:");

                            foreach (var label in labelNodes.EnumerateArray())
                            {
                                Console.WriteLine($"\t   {label.GetProperty("title").GetString()}");

                            }
                            Console.WriteLine();

                        }
                    }
                }
            }
        }
    }
}
