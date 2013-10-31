using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;

namespace Zen.DataStore.NHibernate
{
    public class BasicNHibernateRepository<TEntity> : IRepository<TEntity> where TEntity: class, IHasStringId
    {
        private readonly ISession _session;
        private readonly ITransaction _transaction;

        public BasicNHibernateRepository(ISession session)
        {
            _session = session;
            _transaction = _session.BeginTransaction();
        }
        
        public BasicNHibernateRepository(ISession session, ITransaction transaction)
        {
            _session = session;
            //_transaction = transaction;
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
        public void Dispose()
        {
            if (_transaction != null) _transaction.Dispose();
            /*if (_session != null)
            {
                _session.Dispose();
            }*/
        }

        /// <summary>
        ///     Постоить запрос
        /// </summary>
        public IQueryable<TEntity> Query { get { return _session.Query<TEntity>(); } }

        /// <summary>
        ///     Найти объект БД по строковому ИД
        /// </summary>
        /// <param name="id">Ид объекта</param>
        /// <returns>Объект из БД</returns>
        public TEntity Find(string id)
        {
            return Query.First(e => e.Id == id);
        }

        public IQueryable<TEntity> Find(IEnumerable<string> ids)
        {
            return Query.Where(e => ids.Contains(e.Id));
        }

        /// <summary>
        ///     Сохранить объект в БД
        /// </summary>
        /// <param name="entity">Объект</param>
        public void Store(TEntity entity)
        {
            _session.SaveOrUpdate(entity);
        }

        public void StoreBulk(IEnumerable<TEntity> entities)
        {
            var innerSession = _session;
            var tr = innerSession.BeginTransaction();
            foreach (var entity in entities)
            {
                innerSession.SaveOrUpdate(entity);
            }
            tr.Commit();
        }

        /// <summary>
        ///     Удалить объект из БД
        /// </summary>
        /// <param name="entity">Объект</param>
        public void Delete(TEntity entity)
        {
            _session.Delete(entity);
        }

        /// <summary>
        ///     Сохранить изменения сессии
        /// </summary>
        public void SaveChanges()
        {
            _transaction.Commit();
        }

        public void DeleteAttach(string key)
        {
            throw new NotImplementedException();
        }

        public void Detach(TEntity entity)
        {            
            throw new NotImplementedException();
        }

        public void DeleteById(string id)
        {
            var ent = Find(id);
            Delete(ent);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return Query;
        }
    }
}