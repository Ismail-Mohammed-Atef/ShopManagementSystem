using DataLayer;
using DataLayer.Models;

namespace SystemApp.Repository
{
    public class ProductRepository : GenericRepository<Product> , IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
            
        }
    }
}
