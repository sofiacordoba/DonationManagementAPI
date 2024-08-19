using DonationManagementAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace DonationManagementAPI.Data
{
    public class DonationContext : DbContext
    {
        public DonationContext(DbContextOptions<DonationContext> options) : base(options)
        {
        }

        public DbSet<Donor> Donors { get; set; } 
        public DbSet<Payment> Payments { get; set; } 
        public DbSet<Pledge> Pledges { get; set; }
        public DbSet<PaymentPledge> PaymentPledges { get; set; }
        public DbSet<Changelog> Changelogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentPledge>()
                .HasKey(pp => new { pp.PaymentId, pp.PledgeId });

            modelBuilder.Entity<PaymentPledge>()
                .HasOne(pp => pp.Payment)
                .WithMany(p => p.PaymentPledges)
                .HasForeignKey(pp => pp.PaymentId);

            modelBuilder.Entity<PaymentPledge>()
                .HasOne(pp => pp.Pledge)
                .WithMany(p => p.PaymentPledges)
                .HasForeignKey(pp => pp.PledgeId);
        }
    }
}
