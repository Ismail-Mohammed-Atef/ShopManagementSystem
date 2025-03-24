namespace SystemApp.Repository
{
    public interface IGenericRepository<T>
    {
        public Task<IList<T>> GetAllAsync();
        public Task<T> GetById(int id);
        public IQueryable<T> GetAll();
        public void Insert(T obj);
        public void Update(T obj);
        public void Delete(T obj);
        public void Save();
    }
}
