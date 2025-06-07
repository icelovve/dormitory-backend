using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Model
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [ForeignKey("Contract")]
        public int ContractId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public DateTime PaymentDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "รอดำเนินการ"; 

        public Contract? Contract { get; set; }
    }
}
