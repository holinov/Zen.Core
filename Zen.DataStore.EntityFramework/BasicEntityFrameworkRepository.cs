using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;

namespace Zen.DataStore.EntityFramework
{
	public class BasicEntityFrameworkRepository<TEntity> : IRepository<TEntity> where TEntity : class
	{
        protected DbContext _context;
        protected DbSet<TEntity> _dbSet;

        public BasicEntityFrameworkRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public IQueryable<TEntity> Query
        {
            get 
            {
                return _dbSet;
            }
        }

        public TEntity Find(string id)
        {
            return _dbSet.Find(id);
        }

        public IQueryable<TEntity> Find(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public void Store(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Added;
        }

        public void StoreBulk(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public void Delete(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void DeleteAttach(string key)
        {
            throw new NotImplementedException();
        }

        public void Detach(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        public void DeleteById(string id)
        {
            TEntity entityToDelete = _dbSet.Find(id);
            Delete(entityToDelete);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _dbSet;
        }

        public void Dispose()
        {
            
        }
    }
}