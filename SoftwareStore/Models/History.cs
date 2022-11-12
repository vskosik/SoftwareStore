using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwareStore.Models
{
    public class History
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("FK_Product_1234")]
        public int ProductId { get; set; }
        [ForeignKey("FK_User_1234")]
        public int UserId { get; set; }
        public int Price { get; set; }
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; }

    }
}
