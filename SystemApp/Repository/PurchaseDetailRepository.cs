using DataLayer;
using DataLayer.Models;

namespace SystemApp.Repository
{
    public class PurchaseDetailRepository : GenericRepository<PurchaseDetail>,IPurchaseDetailRepository
    {
        public PurchaseDetailRepository(AppDbContext context):base(context)
        {
            
        }
    }
}
