using DonationManagementAPI.Data;
using DonationManagementAPI.DTOs;
using DonationManagementAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DonationManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly DonationContext _context;
        public PaymentController(DonationContext context)
        {
            _context = context;
        }

        ///// <summary>
        ///// Gets a list of all payments, with optional filters for donor ID, amount, and date.
        ///// </summary>
        ///// <param name="donorId">Optional. Filter payments by donor ID.</param>
        ///// <param name="amount">Optional. Filter payments by amount.</param>
        ///// <param name="date">Optional. Filter payments by date.</param>
        ///// <returns>A list of payments that match the specified criteria.</returns>

        [HttpGet(Name = "GetAllPayments")]
        public async Task<ActionResult<List<Payment>>> GetAllPayments(
            [FromQuery] int? donorId = null,
            [FromQuery] decimal? amount = null,
            [FromQuery] DateTime? date = null)
        {
            var query = _context.Payments.AsQueryable();

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

            var payments = await query.ToListAsync();
            return Ok(payments);
        }

        ///// <summary>
        ///// Gets a payment by its ID.
        ///// </summary>
        ///// <param name="id">The ID of the payment to retrieve.</param>
        ///// <returns>The payment with the specified ID, or a 404 error if not found.</returns>

        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound("Payment not found.");
            }
            return Ok(payment);
        }
        ///// <summary>
        ///// Creates a new payment.
        ///// </summary>
        ///// <param name="paymentDTO">The payment data to create.</param>
        ///// <returns>The created payment.</returns>
        ///// 
        [HttpPost]
        public async Task<ActionResult<Payment>> AddPayment(PaymentCreateDto paymentDTO)
        {
            var donorExists = await _context.Donors.AnyAsync(d => d.DonorId == paymentDTO.DonorId);
            if (!donorExists)
            {
                return BadRequest($"Donor with ID {paymentDTO.DonorId} does not exist.");
            }

            var payment = new Payment
            {
                DonorId = paymentDTO.DonorId,
                Amount = paymentDTO.Amount,
                Date = paymentDTO.Date
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var userName = User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            var changeLog = new Changelog
            {
                EntityName = "Payment",
                EntityId = payment.PaymentId,
                ChangeType = "INSERT",
                ChangedData = $"Payment with ID {payment.PaymentId} created",
                ChangeDate = DateTime.UtcNow,
                UserName = userName
            };

            _context.Changelogs.Add(changeLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentId }, payment);
        }

        ///// <summary>
        ///// Updates an existing payment by its ID.
        ///// </summary>
        ///// <param name="id">The ID of the payment to update.</param>
        ///// <param name="paymentDTO">The updated payment data.</param>
        ///// <returns>A status message indicating the result of the update operation.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePayment(int id, PaymentUpdateDto paymentDTO)
        {
            var dbPayment = await _context.Payments.FindAsync(id);
            if (dbPayment == null)
            {
                return NotFound("Payment not found.");
            }

            var originalPayment = new Payment
            {
                DonorId = dbPayment.DonorId,
                Amount = dbPayment.Amount,
                Date = dbPayment.Date
            };

            if (paymentDTO.DonorId.HasValue) dbPayment.DonorId = paymentDTO.DonorId.Value;
            if (paymentDTO.Amount.HasValue) dbPayment.Amount = paymentDTO.Amount.Value;
            if (paymentDTO.Date.HasValue) dbPayment.Date = paymentDTO.Date.Value;

            bool hasChanges = !(
                originalPayment.DonorId == dbPayment.DonorId &&
                originalPayment.Amount == dbPayment.Amount &&
                originalPayment.Date == dbPayment.Date
            );

            if (hasChanges)
            {
                var userName = User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

                var changeLog = new Changelog
                {
                    EntityName = "Payment",
                    EntityId = id,
                    ChangeType = "UPDATE",
                    ChangedData = $"Payment with ID {id} updated. " +
                                  $"Original Data: DonorId={originalPayment.DonorId}, Amount={originalPayment.Amount}, Date={originalPayment.Date}. " +
                                  $"Updated Data: DonorId={dbPayment.DonorId}, Amount={dbPayment.Amount}, Date={dbPayment.Date}.",
                    ChangeDate = DateTime.UtcNow,
                    UserName = userName
                };

                _context.Changelogs.Add(changeLog);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            return Ok($"Payment with ID {id} was successfully updated.");
        }

        ///// <summary>
        ///// Deletes a payment by its ID.
        ///// </summary>
        ///// <param name="id">The ID of the payment to delete.</param>
        ///// <returns>A status message indicating the result of the delete operation.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var dbPayment = await _context.Payments
                .Include(p => p.PaymentPledges)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (dbPayment == null)
            {
                return NotFound("Payment not found.");
            }

            if (dbPayment.PaymentPledges.Any())
            {
                return Conflict("Payment cannot be deleted because there are pledges associated with it.");
            }

            var userName = User?.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            var changeLog = new Changelog
            {
                EntityName = "Payment",
                EntityId = id,
                ChangeType = "DELETE",
                ChangedData = $"Payment with ID {id} deleted.",
                ChangeDate = DateTime.UtcNow,
                UserName = userName
            };

            _context.Changelogs.Add(changeLog);

            _context.Payments.Remove(dbPayment);
            await _context.SaveChangesAsync();

            return Ok($"Payment with ID {id} was successfully deleted.");
        }
        ///// <summary>
        ///// Associates a pledge with a payment.
        ///// </summary>
        ///// <param name="paymentId">The ID of the payment.</param>
        ///// <param name="pledgeId">The ID of the pledge to associate.</param>
        ///// <returns>No content if the association is successful, or an error if not.</returns>
        /// 
        [HttpPost("{paymentId}/Pledges/{pledgeId}")]
        public async Task<IActionResult> AddPledgeToPayment(int paymentId, int pledgeId) 
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return NotFound("Payment not found.");
            }

            var pledge = await _context.Pledges.FindAsync(pledgeId);
            if (pledge == null)
            {
                return NotFound("Pledge not found.");
            }

            var paymentPledge = new PaymentPledge
            {
                PaymentId = paymentId,
                PledgeId = pledgeId
            };

            _context.PaymentPledges.Add(paymentPledge);

            var userName = User?.Identity?.Name ?? "Unknown";
            var changeLog = new Changelog
            {
                EntityName = "PaymentPledge",
                EntityId = paymentId, 
                ChangeType = "INSERT",
                ChangedData = $"Pledge {pledgeId} associated with Payment {paymentId}.",
                ChangeDate = DateTime.UtcNow,
                UserName = userName
            };
            _context.Changelogs.Add(changeLog);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        ///// <summary>
        ///// Disassociates a pledge from a payment.
        ///// </summary>
        ///// <param name="paymentId">The ID of the payment.</param>
        ///// <param name="pledgeId">The ID of the pledge to disassociate.</param>
        ///// <returns>No content if the disassociation is successful, or an error if not.</returns>
        /// 
        [HttpDelete("{paymentId}/Pledges/{pledgeId}")]
        public async Task<IActionResult> RemovePledgeFromPayment(int paymentId, int pledgeId) 
        {
            var paymentPledge = await _context.PaymentPledges
                .Where(pp => pp.PaymentId == paymentId && pp.PledgeId == pledgeId)
                .FirstOrDefaultAsync();

            if (paymentPledge == null)
            {
                return NotFound("Payment-Pledge association not found.");
            }

            _context.PaymentPledges.Remove(paymentPledge);

            var userName = User?.Identity?.Name ?? "Unknown";
            var changeLog = new Changelog
            {
                EntityName = "PaymentPledge",
                EntityId = paymentId,
                ChangeType = "DELETE",
                ChangedData = $"Pledge {pledgeId} disassociated from Payment {paymentId}.",
                ChangeDate = DateTime.UtcNow,
                UserName = userName
            };
            _context.Changelogs.Add(changeLog);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        ///// <summary>
        ///// Gets a list of pledges associated with a specific payment.
        ///// </summary>
        ///// <param name="paymentId">The ID of the payment whose pledges are to be retrieved.</param>
        ///// <returns>A list of pledges associated with the specified payment, or a 404 error if the payment is not found.</returns>
        [HttpGet("{paymentId}/Pledges")]
        public async Task<ActionResult<IEnumerable<Pledge>>> GetPledgesForPayment(int paymentId) 
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentPledges)
                .ThenInclude(pp => pp.Pledge)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return NotFound("Payment not found.");
            }

            return Ok(payment.PaymentPledges.Select(pp => pp.Pledge));
        }
    }
}