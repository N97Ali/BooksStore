using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Books.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        [DisplayName("Category Name")]
        public string? Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1,10,ErrorMessage ="Enter the Curect Order")]
        public int DisplayOrder { get; set; }
        
    }
}
