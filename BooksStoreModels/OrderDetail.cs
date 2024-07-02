using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Books.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        [Required]
        public int OrderHeaderId { get; set; }
        [ForeignKey("OrderHeaderId")]
        [ValidateNever]
        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }
        public int count { get; set; }
        public double price { get; set; }   
    }
}
