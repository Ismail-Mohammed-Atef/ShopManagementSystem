using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class Product : BaseEntity
    {
        public int Id { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        [Range(0, double.MaxValue)]

        public double Quantity { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double buyPrice { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double SellPrice { get; set; }

    }
}
