using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class ExtraPayments : BaseEntity
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Cost { get; set; }
        public DateTime Date { get; set; }
    }
}
