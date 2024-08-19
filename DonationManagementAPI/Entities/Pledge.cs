using System.ComponentModel.DataAnnotations;

namespace DonationManagementAPI.Entities
{
    public class Pledge
    {
        public int PledgeId { get; set; }

        [Required]
        public int DonorId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public Donor Donor { get; set; }
        public ICollection<PaymentPledge> PaymentPledges { get; set; }
    }
}
