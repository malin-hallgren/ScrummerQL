using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL.Repositories
{
    internal interface IMilestoneRepository
    {
        Task<Milestone?> GetByIdAsync(int id);
        Task<List<Milestone>> GetAllAsync();
        Task AddAsync(Milestone milestone);
        Task UpdateAsync(Milestone milestone);
        Task DeleteAsync(int id);
        Task<bool> ExistsByGitLabIIdAsync(int id);
    }
}
