using ScrummerQL.Data;
using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScrummerQL.Repositories
{
    internal class MilestoneRepository
    {
        private  readonly ScrummerQLDbContext _context;
        Task<Milestone?> GetByIdAsync(int id);
        Task<List<Milestone>> GetAllAsync();
        Task AddAsync(Milestone milestone);
        Task UpdatAsync(Milestone milestone);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
