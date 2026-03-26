using ScrummerQL.Data;
using ScrummerQL.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ScrummerQL.Repositories
{
    internal class MilestoneRepository : IMilestoneRepository
    {
        private readonly ScrummerQLDbContext _context;

        public MilestoneRepository(ScrummerQLDbContext context)
        {
            _context = context;
        }

        public async Task<Milestone?> GetByIdAsync(int id)
        {
            return await _context.Milestones.FindAsync(id);
        }
        public async Task<List<Milestone>> GetAllAsync()
        {
            return await _context.Milestones.ToListAsync();
        }
        public async Task AddAsync(Milestone milestone)
        {
            await _context.Milestones.AddAsync(milestone);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Milestone milestone)
        {
            _context.Milestones.Update(milestone);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var toRemove = await GetByIdAsync(id);
            if (toRemove != null)
            {
                _context.Milestones.Remove(toRemove);
                await _context.SaveChangesAsync();
            }
            
        }
        public async Task<bool> ExistsByGitLabIIdAsync(int id)
        {
            return await _context.Milestones.AnyAsync(m => m.GitLabIId == id);
        }
    }
}
