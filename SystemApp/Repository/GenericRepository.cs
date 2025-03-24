using DataLayer;
using DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace SystemApp.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }
        public void Delete(T obj)
        {
            var model = _context.Set<T>().Remove(obj);
        }

        public async Task<IList<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public IQueryable<T> GetAll()
        {
            return _context.Set<T>();
        }

        public async Task<T> GetById(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public void Insert(T obj)
        {
            _context.Set<T>().Add(obj);
        }

        public void Update(T obj)
        {
            _context.Set<T>().Update(obj);
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
