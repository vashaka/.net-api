namespace nett.Dtos
{
    public partial class UserForRegistrationDto
    {
        public string Email { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string PasswordConfirm { get; set; } = String.Empty;
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string Gender { get; set; }
        public UserForRegistrationDto()
        {
            Gender ??= "";
        }
    }
}