using DonationManagementAPI.Data;
using DonationManagementAPI.DTOs;
using DonationManagementAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DonationManagementAPI.Controllers
{
    namespace DonationManagementAPI.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class PledgeController : ControllerBase
        {
            private readonly DonationContext _context;

            public PledgeController(DonationContext context)
            {
                _context = context;
            }

            [HttpGet(Name = "GetAllPledges")]
            public async Task<ActionResult<List<Pledge>>> GetAllPledges(
                [FromQuery] int? donorId = null,
                [FromQuery] decimal? amount = null,
                [FromQuery] DateTime? date = null)
            {
                var query = _context.Pledges.AsQueryable();

                if (donorId.HasValue)
                {
                    query = query.Where(p => p.DonorId == donorId.Value);
                }

                if (amount.HasValue)
                {
                    query = query.Where(p => p.Amount == amount.Value);
                }

                if (date.HasValue)
                {
                    query = query.Where(p => p.Date.Date == date.Value.Date);
                }

                var pledges = await query.ToListAsync();

                return Ok(pledges);
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<Pledge>> GetPledge(int id)
            {
                var pledge = await _context.Pledges.FindAsync(id);
                if (pledge == null)
                {
                    return NotFound("Pledge not found.");
                }

                return Ok(pledge);
            }

            [HttpPost]
            public async Task<ActionResult<Pledge>> AddPledge(PledgeCreateDto pledgeDTO)
            {
                // Validate if donor exists
                var donorExists = await _context.Donors.AnyAsync(d => d.DonorId == pledgeDTO.DonorId);
                if (!donorExists)
                {
                    return BadRequest("Donor does not exist.");
                }

                // Map DTO to entity
                var pledge = new Pledge
                {
                    DonorId = pledgeDTO.DonorId,
                    Amount = pledgeDTO.Amount,
                    Date = pledgeDTO.Date
                };

                // Get username and handle null case
                var userName = User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

                // Log creation in the changelog
                var changeLog = new Changelog
                {
                    EntityName = "Pledge",
                    EntityId = pledge.PledgeId,
                    ChangeType = "INSERT",
                    ChangedData = $"Pledge with ID {pledge.PledgeId} created.",
                    ChangeDate = DateTime.UtcNow,
                    UserName = userName
                };

                _context.Changelogs.Add(changeLog);

                // Add new pledge to context
                _context.Pledges.Add(pledge);
                await _context.SaveChangesAsync();

                // Return newly added pledge with 201 Created
                return CreatedAtAction(nameof(GetPledge), new { id = pledge.PledgeId }, pledge);
            }

            [HttpPut("{id}")]
            public async Task<ActionResult> UpdatePledge(int id, PledgeUpdateDto pledgeDTO) 
            {
                var dbPledge = await _context.Pledges.FindAsync(id);
                if (dbPledge == null)
                {
                    return NotFound("Pledge not found.");
                }

                // Map properties from DTO to existing pledge
                if (pledgeDTO.DonorId.HasValue) dbPledge.DonorId = pledgeDTO.DonorId.Value;
                if (pledgeDTO.Amount.HasValue) dbPledge.Amount = pledgeDTO.Amount.Value;
                if (pledgeDTO.Date.HasValue) dbPledge.Date = pledgeDTO.Date.Value;

                // Get username and handle null case
                var userName = User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

                // Log update in the changelog
                var changeLog = new Changelog
                {
                    EntityName = "Pledge",
                    EntityId = id,
                    ChangeType = "UPDATE",
                    ChangedData = $"Pledge with ID {id} updated.",
                    ChangeDate = DateTime.UtcNow,
                    UserName = userName
                };

                _context.Changelogs.Add(changeLog);
                await _context.SaveChangesAsync();

                // Return confirmation message
                return Ok($"Pledge with ID {id} was successfully updated.");
            }

            [HttpDelete("{id}")]
            public async Task<ActionResult> DeletePledge(int id)
            {
                // Find the pledge
                var dbPledge = await _context.Pledges
                    .Include(p => p.PaymentPledges)
                    .FirstOrDefaultAsync(p => p.PledgeId == id);

                if (dbPledge == null)
                {
                    return NotFound("Pledge not found.");
                }

                // Check if the pledge has associated payments
                if (dbPledge.PaymentPledges.Any())
                {
                    return Conflict("Pledge cannot be deleted because there are payments associated with it.");
                }

                // Get username and handle null case
                var userName = User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

                // Log deletion in the changelog
                var changeLog = new Changelog
                {
                    EntityName = "Pledge",
                    EntityId = id,
                    ChangeType = "DELETE",
                    ChangedData = $"Pledge with ID {id} deleted.",
                    ChangeDate = DateTime.UtcNow,
                    UserName = userName
                };

                _context.Changelogs.Add(changeLog);

                // Remove pledge
                _context.Pledges.Remove(dbPledge);
                await _context.SaveChangesAsync();

                return Ok($"Pledge with ID {id} was successfully deleted.");
            }

            //Lists all Payments associated with a Pledge
            [HttpGet("{pledgeId}/Payments")]
            public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsForPledge(int pledgeId) 
            {
                var pledge = await _context.Pledges
                    .Include(p => p.PaymentPledges)
                    .ThenInclude(pp => pp.Payment)
                    .FirstOrDefaultAsync(p => p.PledgeId == pledgeId);

                if (pledge == null)
                {
                    return NotFound("Pledge not found.");
                }

                return Ok(pledge.PaymentPledges.Select(pp => pp.Payment));
            }
        }
    }
}
