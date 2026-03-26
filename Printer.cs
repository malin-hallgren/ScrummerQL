using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ScrummerQL.Model; 

namespace ScrummerQL
{
    internal class Printer
    {
        public static void PrintByMilestone(List<Milestone> milestones, List<Issue>  issues)
        {
            for ( int i = 0; i < milestones.Count; i++ )
            {
                if (i == milestones.Count - 1)
                {
                    Console.WriteLine($"Milestone: {milestones[i].Title} (Start: {milestones[i].StartDate}, End: {(milestones[i].EndDate.HasValue ? milestones[i].EndDate.Value.ToString() : "N/A")}\nCompleted Points: {milestones[i].CompletedPoints}/{milestones[i].TotalPoints})\n");
                    foreach (var issue in milestones[i].Issues)
                    {
                        Console.WriteLine($"- Issue: {issue.Title}\n   State: {issue.State}");
                        foreach (var childIssue in issue.ChildIssues)
                        {
                            Console.WriteLine($"    - Child Issue: {childIssue.Title}\n\tPoints: {childIssue.Points}\n\tTeam: {childIssue.Team}\n\tPriority: {childIssue.Priority}\n\tStatus: {childIssue.Status}\n\tState: {childIssue.State}\n");
                        }
                    }
                }

                else
                {
                    Console.WriteLine($"Milestone: {milestones[i].Title} (Start: {milestones[i].StartDate}, End: {(milestones[i].EndDate.HasValue ? milestones[i].EndDate.Value.ToString() : "N/A")}\nCompleted Points: {milestones[i].CompletedPoints}/{milestones[i].TotalPoints})\n");
                }
            }
        }

        public static void PrintIssuesWithoutMilestone(List<Issue> issues)
        {

            Console.WriteLine("Issues without Milestone:\n");
            foreach (var issue in issues)
            {
                
                Console.WriteLine($"- Issue: {issue.Title}\n   State: {issue.State}");
                foreach (var childIssue in issue.ChildIssues)
                {
                    Console.WriteLine($"    - Child Issue: {childIssue.Title}\n\tPoints: {childIssue.Points}\n\tTeam: {childIssue.Team}\n\tPriority: {childIssue.Priority}\n\tStatus: {childIssue.Status}\n\tState: {childIssue.State}\n");
                }
            }
        }
    }
}
