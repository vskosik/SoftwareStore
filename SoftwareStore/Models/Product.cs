using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwareStore.Models
{
    public class Product : BaseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Rate { get; set; }
        public int Price { get; set; }
        [ForeignKey("FK_Vendor_1")]
        public int VendorId { get; set; }
        public int Qty { get; set; }
        //virtual properties
        public Vendor Vendor { get; set; }

    }
}
