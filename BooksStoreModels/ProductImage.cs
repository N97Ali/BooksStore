﻿ 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 

namespace Books.Models
{
    public class ProductImage
    {
        public  int id  { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
