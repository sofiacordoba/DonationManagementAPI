using System.ComponentModel.DataAnnotations;

namespace DonationManagementAPI.Entities
{
    public class Changelog
    {
        public int ChangelogId { get; set; }

        [Required]
        public string EntityName { get; set; }

        [Required]
        public int EntityId { get; set; }

        [Required]
        public string ChangeType { get; set; }

        [Required]
        public string ChangedData { get; set; }

        [Required]
        public DateTime ChangeDate { get; set; }

        [Required]
        public string UserName { get; set; }
    }
}
