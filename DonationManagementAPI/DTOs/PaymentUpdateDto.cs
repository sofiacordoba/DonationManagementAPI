using System;
using System.ComponentModel.DataAnnotations;

namespace DonationManagementAPI.DTOs
{
    public class PaymentUpdateDto
    {
        public int? DonorId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal? Amount { get; set; }

        public DateTime? Date { get; set; }
    }
}