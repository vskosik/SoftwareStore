using System.ComponentModel;

namespace SoftwareStore.Models
{
    public class User : BaseModel
    {
        [DisplayName("First Name")] public string FirstName { get; set; }

        [DisplayName("Last Name")] public string LastName { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}