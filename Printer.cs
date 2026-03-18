using System;
using System.Collections.Generic;
using System.Text;
using ScrummerQL.Model; 

namespace ScrummerQL
{
    internal class Printer
    {
        public static void PrintByMilestone(List<Milestone> milestonesWithIssues)
        {
            for ( int i = 0; i < milestonesWithIssues.Count; i++ )
            {
                if (i == milestonesWithIssues.Count - 1)
                {
                    Console.WriteLine($"Milestone: {milestonesWithIssues[i].Title} (Start: {milestonesWithIssues[i].StartDate}, End: {(milestonesWithIssues[i].EndDate.HasValue ? milestonesWithIssues[i].EndDate.Value.ToString() : "N/A")}\nCompleted Points: {milestonesWithIssues[i].CompletedPoints}/{milestonesWithIssues[i].TotalPoints})\n");
                    foreach (var issue in milestonesWithIssues[i].Issues)
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
                    Console.WriteLine($"Milestone: {milestonesWithIssues[i].Title} (Start: {milestonesWithIssues[i].StartDate}, End: {(milestonesWithIssues[i].EndDate.HasValue ? milestonesWithIssues[i].EndDate.Value.ToString() : "N/A")}\nCompleted Points: {milestonesWithIssues[i].CompletedPoints}/{milestonesWithIssues[i].TotalPoints})\n");
                }
            }
            //foreach (var milestone in milestonesWithIssues)
            //{
            //    Console.WriteLine($"Milestone: {milestone.Title} (Start: {milestone.StartDate}, End: {(milestone.EndDate.HasValue ? milestone.EndDate.Value.ToString() : "N/A")}\nCompleted Points: {milestone.CompletedPoints}/{milestone.TotalPoints})\n");
            //    foreach (var issue in milestone.Issues)
            //    {
            //        Console.WriteLine($"- Issue: {issue.Title}\n   State: {issue.State}");
            //        foreach (var childIssue in issue.ChildIssues)
            //        {
            //            Console.WriteLine($"    - Child Issue: {childIssue.Title}\n\tPoints: {childIssue.Points}\n\tTeam: {childIssue.Team}\n\tPriority: {childIssue.Priority}\n\tStatus: {childIssue.Status}\n\tState: {childIssue.State}\n");
            //        }
            //    }
            //}
        }
    }
}
