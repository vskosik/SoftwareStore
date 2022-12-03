using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SoftwareStore.Models
{
    public class Cart : BaseModel
    {
        [ForeignKey("FK_Product_123")]
        public int ProductId { get; set; }
        [ForeignKey("FK_User_123")]
        public int UserId { get; set; }
        //virtual properties
        public Product Product { get; set; }
        public User User { get; set; }
    }
}
