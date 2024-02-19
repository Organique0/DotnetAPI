namespace DotnetAPI.Dtos
{
    //Object used to transfer data from one place to another
    public partial class UserToAddDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public bool Active { get; set; }

        public UserToAddDTO()
        {
            FirstName ??= "";
            LastName ??= "";
            Email ??= "";
            Gender ??= "";
        }
    }
}