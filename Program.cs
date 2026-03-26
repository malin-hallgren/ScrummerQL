using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using ScrummerQL.Model;
using Microsoft.Extensions.DependencyInjection;
using ScrummerQL.Services;
using ScrummerQL.ResponseHelpers;


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

            var services = new ServiceCollection();

            services.AddHttpClient<QLResponseHandler>();
            services.AddSingleton<IIssueService, IssueService>();
            services.AddSingleton<IMilestoneService, MilestoneService>();   
            services.AddSingleton<IIssueMilestoneLinkService, IssueMilestoneLinkService>();


            using var  provider = services.BuildServiceProvider();

            var qlHandler = provider.GetRequiredService<QLResponseHandler>();
            var issueService = provider.GetRequiredService<IIssueService>();
            var milestoneService = provider.GetRequiredService<IMilestoneService>();
            var linkService = provider.GetRequiredService<IIssueMilestoneLinkService>();

            var json = await qlHandler.GetResponseAsync(query);


            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("Empty response returned from GraphQL.");
                return;
            }

            GraphQLResponse? graphQlResponse = QLResponseValidator.ValidateResponse(json);

            if (graphQlResponse.errors != null)
            {
                foreach (var error in graphQlResponse.errors)
                    Console.WriteLine(error.message);

                return;
            }

            var milestoneList = milestoneService.GetMilestones(graphQlResponse);
            var issueList = issueService.GetIssues(graphQlResponse);

            linkService.LinkIssuesToMilestones(issueList, milestoneList);

            //var (milestoneList, issueList) = Parser.Parse(graphQlResponse!);

            Printer.PrintByMilestone(milestoneList, issueList);
        }
    }
}
