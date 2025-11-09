namespace CarRentals.API.Models
{
    public class ReservationRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public CarType CarType { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ApiKey { get; set; } = string.Empty;
    }
}
