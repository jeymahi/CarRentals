namespace CarRentals.API.Security
{
    public class ApiKeyValidator
    {
        private readonly string _key;

        public ApiKeyValidator(string key)
        {
            _key = key;
        }

        public bool IsValid(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && key == _key;
        }
    }
}
