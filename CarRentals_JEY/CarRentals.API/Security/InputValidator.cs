namespace CarRentals.API.Security
{
    public static class InputValidator
    {
        public static void EnsureDateRange(DateTime start, DateTime end)
        {
            if (start >= end)
                throw new ArgumentException("Start date must be before end date!");
        }

        public static void EnsureCustomer(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("Customer is required!");
        }
    }
}
