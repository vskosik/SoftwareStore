using System.ComponentModel.DataAnnotations;

namespace SoftwareStore.Models
{
    public abstract class BaseModel
    {
        [Key]
        public int Id { get; set; }
    }
}
