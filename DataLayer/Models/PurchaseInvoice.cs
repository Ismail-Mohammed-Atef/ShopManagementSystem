using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class PurchaseInvoice : BaseEntity
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Note { get; set; }

        public ICollection<PurchaseDetail> PurchaseDetails { get; set; }
    }
}
