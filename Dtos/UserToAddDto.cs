namespace nett.Dtos
{
    public partial class UserToAddDto
    {
        public string FirstName { get; set; } = String.Empty;
        public string LastName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Gender { get; set; } = String.Empty;
        public bool Active { get; set; }

        public UserToAddDto()
        {
            Gender ??= "";
        }
    }
}