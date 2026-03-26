using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using ScrummerQL.Model;
using Microsoft.Extensions.DependencyInjection;
using ScrummerQL.Services;
using ScrummerQL.ResponseHelpers;
using ScrummerQL.Data;
using Microsoft.EntityFrameworkCore;
using ScrummerQL.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;


namespace ScrummerQL
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var (token, gitlabUrl, connectionString) = EnvConfig.Config();

            string query = $$"""
            query {
              project(fullPath: "{{gitlabUrl}}") {
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
            }
            """;



            var services = new ServiceCollection();

            services.AddHttpClient<QLResponseHandler>();
            services.AddScoped<IIssueService, IssueService>();
            services.AddScoped<IMilestoneService, MilestoneService>();
            services.AddScoped<IIssueMilestoneLinkService, IssueMilestoneLinkService>();

            services.AddDbContext<ScrummerQLDbContext>((options) =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IIssueRepository, IssueRepository>();
            services.AddScoped<IMilestoneRepository, MilestoneRepository>();


            using var  provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            var qlHandler = provider.GetRequiredService<QLResponseHandler>();
            var issueService = provider.GetRequiredService<IIssueService>();
            var milestoneService = provider.GetRequiredService<IMilestoneService>();
            var linkService = provider.GetRequiredService<IIssueMilestoneLinkService>();

            var json = await qlHandler.GetResponseAsync(query, token);


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

            await milestoneService.SaveClosedMilestonesAsync(milestoneList);
            await issueService.SaveIssuesAsync(issueList);

            var allChildIssues = issueList.SelectMany(i => i.ChildIssues).ToList();

            await issueService.SaveChildIssuesAsync(issueList);

            Printer.PrintByMilestone(milestoneList);
        }
    }
}
