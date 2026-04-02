using System;
using System.Collections.Generic;
using System.Text;
using ScrummerQL.Model;

namespace ScrummerQL.Repositories
{
    internal interface IIssueRepository
    {
        Task<Issue?> GetByIdAsync(int id);
        Task<Issue?> GetByGitLabIIdAsync(int gitLabIId);
        Task<List<Issue>> GetAllAsync();
        Task AddAsync(Issue issue);
        Task UpdateAsync(Issue issue);
        Task DeleteAsync(int id);
        Task<bool> ExistsByGitLabIIdAsync(int id);
        Task SaveChildIssuesAsync(List<ChildIssue> childIssues);
    }
}
