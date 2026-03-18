using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL.Model
{
    internal class Milestone
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public int TotalPoints { get; set; }
        public int CompletedPoints { get; set; }

        public List<Issue> Issues { get; set; }
    }
}
