using System;
using System.Collections.Generic;
using System.Text;
using ScrummerQL.Model;

namespace ScrummerQL.Repositories
{
    internal interface IIssueRepository
    {
        Task<Issue?> GetByIdAsync(int id);
        Task<List<Issue>> GetAllAsync();
        Task AddAsync(Issue issue);
        Task UpdateAsync(Issue issue);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
