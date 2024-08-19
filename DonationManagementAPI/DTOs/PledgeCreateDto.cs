using System;
using System.ComponentModel.DataAnnotations;

namespace DonationManagementAPI.DTOs
{
    public class PledgeCreateDto
    {
        [Required]
        public int DonorId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}