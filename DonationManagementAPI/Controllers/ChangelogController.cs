using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DonationManagementAPI.Entities;
using Microsoft.EntityFrameworkCore;
using DonationManagementAPI.Data;

namespace DonationManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangelogController : ControllerBase
    {
        private readonly DonationContext _context;

        public ChangelogController(DonationContext context)
        {
            _context = context;
        }

        // GET: api/Changelog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Changelog>>> GetChangelogs()
        {
            return await _context.Changelogs.ToListAsync();
        }

        // GET: api/Changelog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Changelog>> GetChangelog(int id)
        {
            var changelog = await _context.Changelogs.FindAsync(id);

            if (changelog == null)
            {
                return NotFound($"Changelog with ID {id} not found.");
            }

            return changelog;
        }

        // Optional: GET with pagination and filtering
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Changelog>>> GetChangelogsFiltered(
            string entityName = null,
            string changeType = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Changelogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityName))
            {
                query = query.Where(c => c.EntityName == entityName);
            }

            if (!string.IsNullOrEmpty(changeType))
            {
                query = query.Where(c => c.ChangeType == changeType);
            }

            var pagedResult = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(pagedResult);
        }
    }
}