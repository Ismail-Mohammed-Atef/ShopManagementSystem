using DataLayer;
using DataLayer.Models;

namespace SystemApp.Repository
{
    public class PurchaseInvoiceRepository : GenericRepository<PurchaseInvoice> , IPurchaseInvoiceRepository
    {
        public PurchaseInvoiceRepository(AppDbContext context): base(context)
        {
            
        }
    }
}
