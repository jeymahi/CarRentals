namespace CarRentals.API.Models
{
    public class Reservation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CustomerId { get; set; } = string.Empty;
        public CarType CarType { get; set; }
        public DateTime Start { get; set; }  
        public DateTime End { get; set; }    
    }
}
