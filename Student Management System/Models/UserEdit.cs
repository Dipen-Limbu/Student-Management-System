namespace Student_Management_System.Models
{
    public class UserEdit
    {
        public int UserId { get; set; }

        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = null!;
    }
}
