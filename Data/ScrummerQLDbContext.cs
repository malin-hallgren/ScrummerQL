using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ScrummerQL.Model;

namespace ScrummerQL.Data
{
    internal class ScrummerQLDbContext : DbContext
    {
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<ChildIssue> ChildIssues { get; set; }

        public ScrummerQLDbContext(DbContextOptions<ScrummerQLDbContext> options)
       : base(options)
        {
        }
    }
}
