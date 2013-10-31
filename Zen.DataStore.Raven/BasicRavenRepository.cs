using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Raven.Abstractions.Commands;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;

namespace Zen.DataStore.Raven
{
    public class BasicRavenRepository<TEntity> : IRepository<TEntity>
        where TEntity : IHasStringId
    {
        private static readonly List<Expression<Func<TEntity, object>>> Includes =
            new List<Expression<Func<TEntity, object>>>();

        private static object _saveLocker = new object();
        private readonly IDocumentSession _session;

        static BasicRavenRepository()
        {
            BuildIncludes();
        }

        public BasicRavenRepository(IDocumentSession session)
        {
            _session = session;
        }

        protected IDocumentSession Session
        {
            get { return _session; }
        }

        public IQueryable<TEntity> Find(IEnumerable<string> ids)
        {
            return Session.Load<TEntity>(ids.ToArray()).AsQueryable();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            //if (_session != null) _session.Dispose();
        }

        /// <summary>
        ///     Найти объект БД по строковому ИД
        /// </summary>
        /// <param name="id">Ид объекта</param>
        /// <returns>Объект из БД</returns>
        public TEntity Find(string id)
        {
            dynamic ldr = MakeIncludes();
            if (ldr != null && id != null) return ldr.Load<TEntity>(id);
            return default(TEntity);
        }

        /// <summary>
        ///     Сохранить объект в БД
        /// </summary>
        /// <param name="entity">Объект</param>
        public void Store(TEntity entity)
        {
            Session.Store(entity);
        }

        /// <summary>
        ///     Удалить объект из БД
        /// </summary>
        /// <param name="entity">Объект</param>
        public void Delete(TEntity entity)
        {
            Session.Delete(entity);
        }

        /// <summary>
        ///     Сохранить изменения сессии
        /// </summary>
        public void SaveChanges()
        {
            lock (_saveLocker)
            {
                Session.SaveChanges();
            }
        }

        /// <summary>
        ///     Постоить запрос
        /// </summary>
        public IQueryable<TEntity> Query
        {
            get
            {
                if (Session != null)
                {
                    IRavenQueryable<TEntity> ss = Session
                        .Query<TEntity>()
                        .Customize(x => x.WaitForNonStaleResultsAsOfNow(new TimeSpan(0, 0, 100)));
                    foreach (var include in Includes)
                    {
                        Expression<Func<TEntity, object>> include1 = include;
                        ss.Customize(x => x.Include(include1));
                    }
                    return ss;
                }
                return null;
            }
        }


        public void DeleteAttach(string key)
        {
            Session.Advanced.DocumentStore.DatabaseCommands.DeleteAttachment(key, null);
        }

        public void StoreBulk(IEnumerable<TEntity> entities)
        {
            int numberOfObjectsThatWarrantChunking = 2000;

            if (entities.Count() < numberOfObjectsThatWarrantChunking)
            {
                foreach (var entity in entities)
                    Session.Store(entity);
                Session.SaveChanges();
                return;
            }

            int numberOfDocumentsPerSession = 1024;

            var objectListInChunks = new List<List<TEntity>>();

            for (int i = 0; i < entities.Count(); i += numberOfDocumentsPerSession)
            {
                objectListInChunks.Add(entities.Skip(i).Take(numberOfDocumentsPerSession).ToList());
            }

            Parallel.ForEach(objectListInChunks, listOfObjects =>
                {
                    using (IDocumentSession ravenSession = Session.Advanced.DocumentStore.OpenSession())
                    {
                        listOfObjects.ForEach(x => ravenSession.Store(x));
                        ravenSession.SaveChanges();
                    }
                });
        }

        /// <summary>
        ///     Постоение списка включений в запрос
        /// </summary>
        private static void BuildIncludes()
        {
            PropertyInfo[] refs = typeof (TEntity).GetProperties()
                                                  .Where(p => p.PropertyType.GetInterface("IRefrence", true) != null)
                                                  .ToArray();


            foreach (var refObject in refs)
            {
                PropertyInfo refObjectIdMemberInfo = refObject.PropertyType.GetProperty("Id");
                ParameterExpression inp = Expression.Parameter(typeof (TEntity));
                MemberExpression accessObj = Expression.MakeMemberAccess(inp, refObject);
                MemberExpression accessObjId = Expression.MakeMemberAccess(accessObj, refObjectIdMemberInfo);
                Expression<Func<TEntity, object>> func = Expression.Lambda<Func<TEntity, object>>(accessObjId, inp);
                Includes.Add(func);
            }
        }

        /// <summary>
        ///     Построить загрузчик с включениями
        /// </summary>
        /// <returns>Загрузчик</returns>
        protected virtual dynamic MakeIncludes()
        {
            if (Session == null)
                return null;

            dynamic loader = Session;
            foreach (var expression in Includes)
            {
                if (loader is IDocumentSession)
                {
                    loader = ((IDocumentSession) loader).Include(expression);
                }
                else
                {
                    loader = ((ILoaderWithInclude<TEntity>) loader).Include(expression);
                }
            }
            return loader;
        }

        public void Detach(TEntity entity)
        {
            Session.Advanced.Evict(entity);
        }


        public IEnumerable<TEntity> GetAll()
        {
            return GetAllFrom(0, new List<TEntity>());
        }

        private List<TEntity> GetAllFrom(int startFrom, List<TEntity> list)
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
                        return GetAllFrom(start, allUsers);
                    }
                }
            }
            return allUsers;
        }

        public IQueryable<TEntity> QueryAll(Expression<Func<TEntity, bool>> queryExpr = null)
        {
            return QueryAll<TEntity>(x => x, queryExpr);
        }
        public IQueryable<TResult> QueryAll<TResult>(Expression<Func<TEntity, TResult>> selectExpr, Expression<Func<TEntity, bool>> queryExpr = null)
        {
            var total = Session.Query<TEntity>().Count();

            var pages = queryAllFrom<TResult>(0, total, new List<IQueryable<TResult>>(), queryExpr, selectExpr);

            var result = new List<TResult>().AsQueryable();

            pages.ForEach(x =>
            {
                result = result.Union(x);
            });

            return result;
        }

        private List<IQueryable<TResult>> queryAllFrom<TResult>(int startFrom, int total, List<IQueryable<TResult>> list, Expression<Func<TEntity, bool>> query, Expression<Func<TEntity, TResult>> selectExpr)
        {
            var allUsers = list;

            using (var session = Session.Advanced.DocumentStore.OpenSession())
            {
                int queryCount = 0;
                int start = startFrom;
                while (true)
                {
                    var current = query == null ? session.Query<TEntity>().Skip(start).Take(1024).Select<TEntity, TResult>(selectExpr) : session.Query<TEntity>().Skip(start).Take(1024).Where(query).Select<TEntity, TResult>(selectExpr);
                    queryCount += 1;
                    allUsers.Add(current);
                    start += 1024;

                    if (start >= total)
                        break;

                    if (queryCount >= session.Advanced.MaxNumberOfRequestsPerSession)
                    {
                        return queryAllFrom(start, total, allUsers, query, selectExpr);
                    }
                }
            }
            return allUsers;
        }

        private List<TEntity> getAllFrom(int startFrom, List<TEntity> list)
        {
            var allUsers = list;

            using (var session = Session.Advanced.DocumentStore.OpenSession())
            {
                int queryCount = 0;
                int start = startFrom;
                while (true)
                {
                    var current = session.Query<TEntity>().Take(1024).Skip(start).ToList();
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
        public void DeleteById(string id)
        {
            Session.Advanced.Defer(new DeleteCommandData { Key = id });
        }
    }
}