using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;

namespace Zen.DataStore.NHibernate
{
    public class BasicNHibernateRepositoryWithGuid<TEntity> : BasicNHibernateRepository<TEntity>,
                                                              IRepositoryWithGuid<TEntity>
        where TEntity : class, IHasGuidId
    {
        public BasicNHibernateRepositoryWithGuid(ISession session) : base(session)
        {
        }

        public BasicNHibernateRepositoryWithGuid(ISession session, ITransaction transaction) : base(session, transaction)
        {
        }

        /// <summary>
        ///     Найти объект по GUID
        /// </summary>
        /// <param name="guid">Уникальный ИД объекта</param>
        /// <returns></returns>
        public TEntity Find(Guid guid)
        {
            return Query.First(e => e.Guid == guid);
        }

        public IQueryable<TEntity> Find(IEnumerable<Guid> guids)
        {
            return Query.Where(e => guids.Contains(e.Guid));
        }

        /// <summary>
        ///     Клонировать документ
        /// </summary>
        /// <param name="entity">Документ, который нужно клонировать</param>
        public void Clone(TEntity entity)
        {
            //throw new NotImplementedException();
        }
    }
}