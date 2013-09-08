using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Commands;
using Raven.Client;

namespace Zen.DataStore.Raven
{
    public class BasicRavenRepositoryWithGuid<TEntity> : BasicRavenRepository<TEntity>,
                                                         IRepositoryWithGuid<TEntity>
        where TEntity : IHasGuidId
    {
        public BasicRavenRepositoryWithGuid(IDocumentSession session) : base(session)
        {
        }

        public IQueryable<TEntity> Find(IEnumerable<Guid> guids)
        {
            //return Session.Query<TEntity>().Where(x => x.Guid.In<Guid>(guids));
            return Session
                .Load<TEntity>(guids.Select(guid => typeof (TEntity).Name + "s/" + guid))
                .ToArray()
                .AsQueryable();
        }

        /// <summary>
        ///     Найти объект по GUID
        /// </summary>
        /// <param name="guid">Уникальный ИД объекта</param>
        /// <returns></returns>
        public TEntity Find(Guid guid)
        {
            return Session.Load<TEntity>(typeof (TEntity).Name + "s/" + guid);
        }

        public void Clone(TEntity entity)
        {
            Session.Advanced.Evict(entity);
            entity.Guid = Guid.NewGuid();
            Session.Store(entity, typeof (TEntity).Name + "s/" + entity.Guid);
        }

        public void Detach(TEntity entity)
        {
            Session.Advanced.Evict(entity);
        }

        public void DeleteById(string id)
        {
            Session.Advanced.Defer(new DeleteCommandData {Key = id});
        }

        public IEnumerable<TEntity> GetAll()
        {
            return getAllFrom(0, new List<TEntity>());
        }

        private List<TEntity> getAllFrom(int startFrom, List<TEntity> list)
        {
            List<TEntity> allUsers = list;

            using (IDocumentSession session = Session.Advanced.DocumentStore.OpenSession())
            {
                int queryCount = 0;
                int start = startFrom;
                while (true)
                {
                    List<TEntity> current = session.Query<TEntity>().Take(1024).Skip(start).ToList();
                    queryCount += 1;
                    if (current.Count == 0)
                        break;

                    start += current.Count;
                    allUsers.AddRange(current);

                    if (queryCount >= session.Advanced.MaxNumberOfRequestsPerSession)
                    {
                        return getAllFrom(start, allUsers);
                    }
                }
            }
            return allUsers;
        }
    }
}