using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ScrummerQL.Data;
using ScrummerQL.Model; 

namespace ScrummerQL
{
    internal class Printer
    {
        public ScrummerQLDbContext context { get; set; }

        public Printer(ScrummerQLDbContext context)
        {
            this.context = context;
        }

        public void PrintByMilestone(List<Milestone> milestones)
        {
            foreach (var milestone in context.Milestones)
            {
                Console.WriteLine($"Milestone: {milestone.Title}\n{milestone.StartDate} - {milestone.EndDate}\nCompleted Points: {milestone.CompletedPoints}/{milestone.TotalPoints}\n");
            }

            Console.WriteLine("------------------------------------------");

            for ( int i = 0; i < milestones.Count; i++ )
            {
                Console.WriteLine($"Milestone: {milestones[i].Title}\n" +
                    $"{milestones[i].StartDate} - " +
                    $"{(milestones[i].EndDate.HasValue ? milestones[i].EndDate.Value.ToString() : "N/A")}\n" +
                    $"\nCompleted Points: {milestones[i].CompletedPoints}/{milestones[i].TotalPoints}\n");
                    foreach (var issue in milestones[i].Issues)
                    {
                        Console.WriteLine($"- Issue: {issue.Title}\n   State: {issue.State}");
                        foreach (var childIssue in issue.ChildIssues)
                        {
                            Console.WriteLine($"    - Child Issue: {childIssue.Title}\n\tPoints: {childIssue.Points}\n\tTeam: {childIssue.Team}\n\tPriority: {childIssue.Priority}\n\tStatus: {childIssue.Status}\n\tState: {childIssue.State}\n");
                        }
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
