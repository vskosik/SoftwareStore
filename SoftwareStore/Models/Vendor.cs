using System.ComponentModel;

namespace SoftwareStore.Models
{
    public class Vendor : BaseModel
    {
        [DisplayName("Vendor Name")] public string VendorName { get; set; }

        public string Email { get; set; }
    }
}