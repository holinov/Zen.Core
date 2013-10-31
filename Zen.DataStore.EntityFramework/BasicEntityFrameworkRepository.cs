using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Zen.DataStore.EntityFramework
{
	public class BasicEntityFrameworkRepository<TEntity> : IRepository<TEntity> where TEntity : HasStringId
	{
        protected DbContext _context;
        protected IDbContextFactory _contextFactory;
        protected DbSet<TEntity> _dbSet;

        public BasicEntityFrameworkRepository(IDbContextFactory contextFactory)
        {
            _context = contextFactory.Create();
            _dbSet = _context.Set<TEntity>();
            _contextFactory = contextFactory;
        }
        public IQueryable<TEntity> QueryAll(Expression<Func<TEntity, bool>> queryExpr = null)
        {
            if (queryExpr != null)
            {
                var flt = queryExpr.Compile();
                return Query.Where(e => flt(e));
            }
            else
            {
                return Query;
            }
        }

        public IQueryable<TResult> QueryAll<TResult>(Expression<Func<TEntity, TResult>> selectExpr,
                                                     Expression<Func<TEntity, bool>> queryExpr = null)
        {
            var q = Query;
            IQueryable<TResult> resq;
            if (queryExpr != null)
            {
                var ex = queryExpr.Compile();
                q = q.Where(e => ex(e));
            }

            var se = selectExpr.Compile();
            resq = q.Select(e => se(e));


            return resq;
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
            return _dbSet.Where(x => ids.Contains(x.Id));
        }

        public void Store(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Added;
        }

        public void StoreBulk(IEnumerable<TEntity> entities)
        {
            using (var bulkContext = _contextFactory.Create())
            {
                bulkContext.Configuration.AutoDetectChangesEnabled = false;
                bulkContext.Configuration.ValidateOnSaveEnabled = false;
                int counter = 0;
                foreach (var entity in entities)
                {
                    bulkContext.Entry(entity).State = EntityState.Added;
                    counter++;
                    if (counter % 100 == 0)
                        bulkContext.SaveChanges();
                }
                bulkContext.SaveChanges();
            }
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