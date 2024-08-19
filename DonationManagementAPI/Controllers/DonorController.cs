using DonationManagementAPI.Data;
using DonationManagementAPI.DTOs;
using DonationManagementAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Security.Claims;

namespace DonationManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonorController : ControllerBase
    {
        private readonly DonationContext _context;
        public DonorController(DonationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all donors, optionally filtered by the provided query parameters.
        /// </summary>
        /// <param name="firstName">Filter by donor's first name.</param>
        /// <param name="lastName">Filter by donor's last name.</param>
        /// <param name="city">Filter by donor's city.</param>
        /// <param name="state">Filter by donor's state.</param>
        /// <param name="country">Filter by donor's country.</param>
        /// <param name="email">Filter by donor's email address.</param>
        /// <param name="phoneNumber">Filter by donor's phone number.</param>
        /// <param name="activeStatus">Filter by donor's active status.</param>
        /// <returns>A list of donors that match the specified filters.</returns>

        [HttpGet(Name = "GetAllDonors")]
        public async Task<ActionResult<List<Donor>>> GetAllDonors( 
            [FromQuery] string? firstName = null,
            [FromQuery] string? lastName = null,
            [FromQuery] string? city = null,
            [FromQuery] string? state = null,
            [FromQuery] string? country = null,
            [FromQuery] string? email = null,
            [FromQuery] string? phoneNumber = null,
            [FromQuery] bool? activeStatus = null)
        {
            var query = _context.Donors.AsQueryable();

            if (!string.IsNullOrEmpty(firstName))
            {
                query = query.Where(d => EF.Functions.Like(d.FirstName.ToLower(), $"%{firstName.ToLower()}%"));
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                query = query.Where(d => EF.Functions.Like(d.LastName.ToLower(), $"%{lastName.ToLower()}%"));
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(d => EF.Functions.Like(d.City.ToLower(), $"%{city.ToLower()}%"));
            }

            if (!string.IsNullOrEmpty(state))
            {
                query = query.Where(d => EF.Functions.Like(d.State.ToLower(), $"%{state.ToLower()}%"));
            }

            if (!string.IsNullOrEmpty(country))
            {
                query = query.Where(d => EF.Functions.Like(d.Country.ToLower(), $"%{country.ToLower()}%"));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(d => EF.Functions.Like(d.Email.ToLower(), $"%{email.ToLower()}%"));
            }

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                query = query.Where(d => EF.Functions.Like(d.PhoneNumber.ToLower(), $"%{phoneNumber.ToLower()}%"));
            }

            if (activeStatus.HasValue)
            {
                query = query.Where(d => d.ActiveStatus == activeStatus.Value);
            }

            var donors = await query.ToListAsync();

            return Ok(donors);
        }

        /// <summary>
        /// Retrieves a single donor by their ID.
        /// </summary>
        /// <param //name="id">The ID of the donor to retrieve.</param>
        /// <returns>The donor with the specified ID, or a 404 status code if not found.</returns>
        /// 
        [HttpGet("{id}")]
        public async Task<ActionResult<Donor>> GetDonor(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor is null)
                return NotFound("Donor not found.");

            return Ok(donor);
        }

        /// <summary>
        /// Adds a new donor to the database.
        /// </summary>
        /// <param //name="donorDTO">The data transfer object containing donor details.</param>
        /// <returns>The created donor, or a conflict status code if a donor with the same email already exists.</returns>
        [HttpPost]
        public async Task<ActionResult<Donor>> AddDonor(DonorCreateDto donorDTO)
        {
            var existingDonor = await _context.Donors
                .FirstOrDefaultAsync(d => d.Email == donorDTO.Email);

            if (existingDonor != null)
            {
                return Conflict("A donor with the same email already exists.");
            }

            var donor = new Donor { 
                FirstName = donorDTO.FirstName,
                LastName = donorDTO.LastName,
                Address = donorDTO.Address,
                City = donorDTO.City,
                State = donorDTO.State,
                PostalCode = donorDTO.PostalCode,
                Country = donorDTO.Country,
                Email = donorDTO.Email,
                PhoneNumber = donorDTO.PhoneNumber,
                ActiveStatus = donorDTO.ActiveStatus 
            };

            var userName = User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            var changeLog = new Changelog
            {
                EntityName = "Donor",
                EntityId = donor.DonorId,
                ChangeType = "INSERT",
                ChangedData = $"Donor with ID {donor.DonorId} created",
                ChangeDate = DateTime.UtcNow,
                UserName = userName
            };

            _context.Changelogs.Add(changeLog);

            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetDonor), new { id = donor.DonorId }, donor);
        }

        /// <summary>
        /// Updates an existing donor's details.
        /// </summary>
        /// <param //name="id">The ID of the donor to update.</param>
        /// <param //name="donorDTO">The data transfer object containing updated donor details.</param>
        /// <returns>A status message indicating the result of the update operation.</returns>

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDonor(int id, DonorUpdateDto donorDTO)
        {
            var dbDonor = await _context.Donors.FindAsync(id);
            if (dbDonor is null)
                return NotFound("Donor not found.");

            if (!string.IsNullOrEmpty(donorDTO.FirstName)) dbDonor.FirstName = donorDTO.FirstName;
            if (!string.IsNullOrEmpty(donorDTO.LastName)) dbDonor.LastName = donorDTO.LastName;
            if (!string.IsNullOrEmpty(donorDTO.Address)) dbDonor.Address = donorDTO.Address;
            if (!string.IsNullOrEmpty(donorDTO.City)) dbDonor.City = donorDTO.City;
            if (!string.IsNullOrEmpty(donorDTO.State)) dbDonor.State = donorDTO.State;
            if (!string.IsNullOrEmpty(donorDTO.PostalCode)) dbDonor.PostalCode = donorDTO.PostalCode;
            if (!string.IsNullOrEmpty(donorDTO.Country)) dbDonor.Country = donorDTO.Country;
            if (!string.IsNullOrEmpty(donorDTO.Email)) dbDonor.Email = donorDTO.Email;
            if (!string.IsNullOrEmpty(donorDTO.PhoneNumber)) dbDonor.PhoneNumber = donorDTO.PhoneNumber;
            if (donorDTO.ActiveStatus.HasValue) dbDonor.ActiveStatus = donorDTO.ActiveStatus.Value;

            var userName = User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            var changeLog = new Changelog
            {
                EntityName = "Donor",
                EntityId = id,
                ChangeType = "UPDATE",
                ChangedData = $"Donor with ID {id} updated.",
                ChangeDate = DateTime.UtcNow,
                UserName = userName
            };

            _context.Changelogs.Add(changeLog);
            await _context.SaveChangesAsync();

            return Ok($"Donor with ID {id} was successfully updated.");
        }

        /// <summary>
        /// Deletes a donor from the database.
        /// </summary>
        /// <param //name="id">The ID of the donor to delete.</param>
        /// <returns>A status message indicating the result of the delete operation.</returns>
        /// 
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDonor(int id) 
        {
            var dbDonor = await _context.Donors
                .Include(d => d.Pledges)
                .Include(d => d.Payments)
                .FirstOrDefaultAsync(d => d.DonorId == id);

            if (dbDonor == null)
                return NotFound("Donor not found.");

            // Check for associated pledges and payments
            if (dbDonor.Pledges.Any())
                return Conflict("Donor cannot be deleted because there are pledges associated with it.");

            if (dbDonor.Payments.Any())
                return Conflict("Donor cannot be deleted because there are payments associated with it.");

            var userName = User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            var changeLog = new Changelog
            {
                EntityName = "Donor",
                EntityId = id,
                ChangeType = "DELETE",
                ChangedData = $"Donor with ID {id} deleted.",
                ChangeDate = DateTime.UtcNow,
                UserName = userName
            };

            _context.Changelogs.Add(changeLog);

            _context.Donors.Remove(dbDonor);
            await _context.SaveChangesAsync();

            return Ok($"Donor with ID {id} was successfully deleted.");
        }
    }
}