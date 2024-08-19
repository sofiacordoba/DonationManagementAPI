namespace DonationManagementAPI.Entities
{
    public class PaymentPledge
    {
        public int PaymentId { get; set; } 
        public Payment Payment { get; set; } = null!; 

        public int PledgeId { get; set; } 
        public Pledge Pledge { get; set; } = null!;
    }
}
