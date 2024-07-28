using UPCI.DAL;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace UPCI.BLL.Services.IService
{
    public interface IRepository<T>
    {
        T GetById(int id);
        List<T> Filter(Func<T, bool> predicate);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task RemoveAsync(T entity);
        Task RemoveRangeAsync(IEnumerable<T> entities);
    }
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }
        public T GetById(int id)
        {
            return _dbContext.Set<T>().Find(id)!;
        }

        public List<T> Filter(Func<T, bool> predicate)
        {
            return _dbContext.Set<T>().Where(predicate).ToList()!;
        }
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _dbContext.UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbContext.UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }
        public async Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        public async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        //SQLRAW

        public async Task Raw_AddAsync(string sql, params object[] parameters)
        {
            var result = _dbContext.Database.ExecuteSqlRaw(sql, parameters);
            await Task.FromResult(result);
        }
        public async Task<T> Create<T>(string sql, params object[] parameters)
        {
            var result = _dbContext.Database.SqlQueryRaw<T>(sql, parameters);
            return await Task.FromResult(result.AsEnumerable().FirstOrDefault()!);
        }
        public async Task<bool> Delete(string sql, params object[] parameters)
        {
            var result = _dbContext.Database.ExecuteSqlRaw(sql, parameters);
            return await Task.FromResult(result > 0);
        }
        public async Task<T> Get<T>(string sql, params object[] parameters)
        {
            var result = _dbContext.Database.SqlQueryRaw<T>(sql, parameters);
            ArgumentNullException.ThrowIfNull(nameof(result));

            return await Task.FromResult(result.AsEnumerable().FirstOrDefault()!);
        }
        public async Task<IQueryable<T>> GetAll<T>(string sql)
        {
            return await Task.FromResult(_dbContext.Database.SqlQueryRaw<T>(sql));
        }
        public async Task<int> Update(string sql, params object[] parameters)
        {
            return await Task.FromResult(_dbContext.Database.ExecuteSqlRaw(sql, parameters));
        }
    }
}
