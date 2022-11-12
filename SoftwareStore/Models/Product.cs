using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwareStore.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Rate { get; set; }
        public int Price { get; set; }
        [ForeignKey("FK_Vendor_1")]
        public int VendorId { get; set; }
        public int State { get; set; }
        //virtual properties
        public Vendor Vendor { get; set; }

    }
}
