using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftwareStore.Models
{
	public class ProductImage
	{
		[Key]
		public int Id { get; set; }
		[ForeignKey("FK_Product_12")]
		public int ProductId { get; set; }
		public byte[] Picture { get; set; }
		//virtual properties
		public Product Product { get; set; }


	}
}
