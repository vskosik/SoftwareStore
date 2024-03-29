﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwareStore.Models
{
    public class Cart : BaseModel
    {
        [ForeignKey("FK_Product_123")] public int ProductId { get; set; }

        [ForeignKey("FK_User_123")] public int UserId { get; set; }

        public int Qty { get; set; }

        //virtual properties
        public Product Product { get; set; }
        public User User { get; set; }
    }
}