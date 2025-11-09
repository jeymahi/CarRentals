using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentals.API.Models
{
    public class CarInventoryEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public CarType CarType { get; set; }

        public int Capacity { get; set; }
    }
}
