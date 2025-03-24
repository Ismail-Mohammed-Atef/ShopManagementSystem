using DataLayer;
using DataLayer.Models;

namespace SystemApp.Repository
{
    public class ExtraPaymentRepository : GenericRepository<ExtraPayments> , IExtraPaymentRepository
    {
        public ExtraPaymentRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
