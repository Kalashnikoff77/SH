namespace Common.Models
{
    public class LoginModel : ModelBase
    {
        public string? Email { get; set; }

        public string? Password { get; set; }

        public bool Remember { get; set; }
    }
}
