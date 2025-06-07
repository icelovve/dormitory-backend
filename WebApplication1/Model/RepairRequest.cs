using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace WebApplication1.Model
{
    public class RepairRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { set; get; }

        [ForeignKey("RoomId")]
        public int RoomId { set; get; }

        public required string Description { set; get; }

        public required string Status { set; get; }

        public DateTime RequestDate { set; get; }

        public Room? Room { get; set; }
    }
}
