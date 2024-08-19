using System;

namespace DonationManagementAPI.DTOs
{
    public class PledgeUpdateDto
    {
        public int? DonorId { get; set; } 
        public decimal? Amount { get; set; } 
        public DateTime? Date { get; set; }
    }
}