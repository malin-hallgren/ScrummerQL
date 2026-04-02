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

            Console.WriteLine($"GitLab URL: {gitlabUrl}");
            Console.WriteLine($"Token length: {token?.Length ?? 0}");

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
                  pageInfo {
                    hasNextPage
                    endCursor
                  }
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
                options.UseSqlServer(connectionString, sqlServerOptionsAction =>
                {
                    sqlServerOptionsAction.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    );
                });
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

            if (graphQlResponse == null)
            {
                Console.WriteLine("Failed to deserialize GraphQL response.");
                return;
            }

            if (graphQlResponse.errors != null)
            {
                foreach (var error in graphQlResponse.errors)
                    Console.WriteLine(error.message);

                return;
            }

            var milestoneList = milestoneService.GetMilestones(graphQlResponse);

            var allIssues = new List<Issue>();
            var issues = issueService.GetIssues(graphQlResponse);
            allIssues.AddRange(issues);

            string? endCursor = ExtractEndCursor(graphQlResponse);
            bool hasNextPage = ExtractHasNextPage(graphQlResponse);

            while (hasNextPage)
            {
                string paginatedQuery = query.Replace("workItems(first: 100)", $"workItems(first: 100, after: \"{endCursor}\")");

                json = await qlHandler.GetResponseAsync(paginatedQuery, token);

                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine("Empty response returned from GraphQL.");
                    break;
                }

                graphQlResponse = QLResponseValidator.ValidateResponse(json);

                if (graphQlResponse.errors != null)
                {
                    foreach (var error in graphQlResponse.errors)
                        Console.WriteLine(error.message);
                    break;
                }

                issues = issueService.GetIssues(graphQlResponse);
                allIssues.AddRange(issues);

                hasNextPage = ExtractHasNextPage(graphQlResponse);
                endCursor = ExtractEndCursor(graphQlResponse);
            }

            var issueList = allIssues;

            linkService.LinkIssuesToMilestones(issueList, milestoneList);

            await milestoneService.SaveClosedMilestonesAsync(milestoneList);
            await issueService.SaveIssuesAsync(issueList);

            var allChildIssues = issueList.SelectMany(i => i.ChildIssues).ToList();

            await issueService.SaveChildIssuesAsync(allChildIssues);

            Printer.PrintByMilestone(milestoneList);
        }

        private static bool ExtractHasNextPage(GraphQLResponse? response)
        {
            return response?.data?.project?.workItems?.pageInfo?.hasNextPage ?? false;
        }

        private static string? ExtractEndCursor(GraphQLResponse? response)
        {
            return response?.data?.project?.workItems?.pageInfo?.endCursor;
        }
    }
}
