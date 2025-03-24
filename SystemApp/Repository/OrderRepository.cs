using DataLayer;
using DataLayer.Models;

namespace SystemApp.Repository
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
