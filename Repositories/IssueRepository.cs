using ScrummerQL.Model;
using Microsoft.EntityFrameworkCore;
using ScrummerQL.Data;

namespace ScrummerQL.Repositories
{
    internal class IssueRepository : IIssueRepository
    {
        private readonly ScrummerQLDbContext _context;

        public IssueRepository(ScrummerQLDbContext context)
        {
            _context = context;
        }

        public async Task<Issue?> GetByIdAsync(int id)
        {
            return await _context.Issues.FindAsync(id);
        }

        public async Task<List<Issue>> GetAllAsync()
        {
            return await _context.Issues.ToListAsync();
        }

        public async Task AddAsync(Issue issue)
        {
            await _context.Issues.AddAsync(issue);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Issue issue)
        {
            _context.Issues.Update(issue);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var issue = await GetByIdAsync(id);
            if (issue != null)
            {
                _context.Issues.Remove(issue);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Issues.AnyAsync(i => i.Id == id);
        }
    }
}
