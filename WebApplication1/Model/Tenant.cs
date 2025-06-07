using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Model
{
    public class Tenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TenantId { get; set; }

        public required string FullName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }
        public required string IDCardNumber { get; set; }

        public required string HouseNumber { get; set; }
        public required string SubDistrict { get; set; }
        public required string District { get; set; }
        public required string Province { get; set; }
        public required string PostalCode { get; set; }

        public required string Username { get; set; }
        public required string PasswordHash { get; set; }

        public required string Status { get; set; }
    }
}
