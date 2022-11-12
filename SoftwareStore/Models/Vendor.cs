using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SoftwareStore.Models
{
    public class Vendor
    {
        [Key]
        public int Id { get; set; }
		[DisplayName("Vendor Name")]
		public string VendorName { get; set; }
        public string Email { get; set; }
    }
}
