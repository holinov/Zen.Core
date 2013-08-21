using Raven.Client;
using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Zen.DataStore.Raven
{
    public class BasicRavenRepository<TEntity> : IRepository<TEntity> 
        where TEntity : IHasStringId
    {
        private readonly IDocumentSession _session;
        private static readonly List<Expression<Func<TEntity, object>>> Includes = new List<Expression<Func<TEntity, object>>>();
        public BasicRavenRepository(IDocumentSession session)
        {
            _session = session;
        }
         static BasicRavenRepository()
         {
             BuildIncludes();             
         }

         public IQueryable<TEntity> Find(IEnumerable<string> ids)
         {
             return Session.Load<TEntity>(ids.ToArray()).AsQueryable();
         }

        /// <summary>
        /// Постоение списка включений в запрос
        /// </summary>
        private static void BuildIncludes()
        {
            var refs = typeof (TEntity).GetProperties()
                .Where(p => p.PropertyType.GetInterface("IRefrence", true) != null)
                .ToArray();


            foreach (var refObject in refs)
            {
                var refObjectIdMemberInfo = refObject.PropertyType.GetProperty("Id");
                var inp = Expression.Parameter(typeof (TEntity));
                var accessObj = Expression.MakeMemberAccess(inp, refObject);
                var accessObjId = Expression.MakeMemberAccess(accessObj, refObjectIdMemberInfo);
                var func = Expression.Lambda<Func<TEntity, object>>(accessObjId, inp);
                Includes.Add(func);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            //if (_session != null) _session.Dispose();
        }

        /// <summary>
        /// Найти объект БД по строковому ИД
        /// </summary>
        /// <param name="id">Ид объекта</param>
        /// <returns>Объект из БД</returns>
        public TEntity Find(string id)
        {
            var ldr = MakeIncludes();
            if (ldr != null && id != null) return ldr.Load<TEntity>(id);
            return default(TEntity);
        }

        /// <summary>
        /// Построить загрузчик с включениями
        /// </summary>
        /// <returns>Загрузчик</returns>
        protected virtual dynamic MakeIncludes()
        {
			if (Session == null)
				return null;

            dynamic loader=Session;
            foreach (var expression in Includes)
            {
                if (loader is IDocumentSession)
                {
                    loader = ((IDocumentSession)loader).Include(expression);
                }
                else
                {
                    loader = ((ILoaderWithInclude<TEntity>)loader).Include(expression);
                }
            }
            return loader;
        }

        /// <summary>
        /// Сохранить объект в БД
        /// </summary>
        /// <param name="entity">Объект</param>
        public void Store(TEntity entity)
        {
          Session.Store(entity);
        }

        /// <summary>
        /// Удалить объект из БД
        /// </summary>
        /// <param name="entity">Объект</param>
        public void Delete(TEntity entity)
        {
            Session.Delete(entity);
        }

        private static object _saveLocker=new object();
        /// <summary>
        /// Сохранить изменения сессии
        /// </summary>
        public void SaveChanges()
        {
            lock (_saveLocker)
            {
                Session.SaveChanges();
            }
        }

        /// <summary>
        /// Постоить запрос
        /// </summary>
        public IQueryable<TEntity> Query
        {
            get
            {
                if (Session != null)
                {
                    var ss = Session
                        .Query<TEntity>()
                        .Customize(x => x.WaitForNonStaleResultsAsOfNow(new TimeSpan(0, 0, 100)));
                    foreach (var include in Includes)
                    {
                        var include1 = include;
                        ss.Customize(x => x.Include(include1));
                    }
                    return ss;
                }
                return null;
            }
        }

        protected IDocumentSession Session
        {
            get { return _session; }
        }


        public void DeleteAttach(string key)
        {
            Session.Advanced.DocumentStore.DatabaseCommands.DeleteAttachment(key, null);
        }

        public void StoreBulk(IEnumerable<TEntity> entities)
        {
            var numberOfObjectsThatWarrantChunking = 2000;

            if (entities.Count() < numberOfObjectsThatWarrantChunking)
            {
                foreach (var entity in entities)
                    Session.Store(entity);
                Session.SaveChanges();
                return;
            }

            var numberOfDocumentsPerSession = 1024;

            List<List<TEntity>> objectListInChunks = new List<List<TEntity>>();

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




    }
}
