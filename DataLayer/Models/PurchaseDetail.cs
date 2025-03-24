using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Models
{
    public class PurchaseDetail : BaseEntity
    {
        public int Id { get; set; }
        public int PurchaseInvoiceId { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public double BuyPrice { get; set; }
        public double SellPrice { get; set; }

        public PurchaseInvoice PurchaseInvoice { get; set; }
    }
}
