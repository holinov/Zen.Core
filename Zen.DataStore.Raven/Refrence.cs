using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Raven.Client;
using JSIgnore = Raven.Imports.Newtonsoft.Json.JsonIgnoreAttribute;

namespace Zen.DataStore
{
    /// <summary>
    ///     Денормализованная ссылка на объект
    /// </summary>
    /// <typeparam name="TRefObject">Тип объекта</typeparam>
    [Serializable]
    public class Refrence<TRefObject> : IRefrence where TRefObject : class, IHasStringId
    {
        private Func<IDocumentSession> _sessionFactory;

        public Refrence()
        {
            _sessionFactory = null;
            RepositoryFactory = null;
        }

        public Refrence(Func<IDocumentSession, IRepository<TRefObject>> repository,
            Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
            RepositoryFactory = repository;
        }


        private TRefObject refObject = default(TRefObject);

        /// <summary>
        ///     Объект на который ссылаемся
        /// </summary>
        [JsonIgnore]
        [JSIgnore]
        public virtual TRefObject Object
        {
            get
            {
                if (RefrenceHacks.SkipRefrences || _sessionFactory == null || RepositoryFactory == null)
                    return default(TRefObject);
                IAppScope scope = null;
                try
                {
                    //При не корректной инициализации создать оторванную сессию
                    if (_sessionFactory == null || RepositoryFactory == null)
                    {
                        scope = AppCore.Instance.BeginScope();
                        _sessionFactory = scope.Resolve<Func<IDocumentSession>>();
                        RepositoryFactory = scope.Resolve<Func<IDocumentSession, IRepository<TRefObject>>>();
                    }

                    using (var sess = GetRefrenceSession())
                    {
                        if (refObject == default(TRefObject) && sess.Repository != null)
                        {
                            refObject = sess.Repository.Find(Id);
                        }

                        return refObject;
                    }
                }
                finally
                {
                    //Если была создана оторванная сессия
                    if (scope != null)
                    {
                        scope.Dispose();
                        _sessionFactory = null;
                        RepositoryFactory = null;
                    }
                }
            }
            set {
                Id = value != null ? value.Id : null;
            }
        }

        /// <summary>
        ///     Репозитарий объектов
        /// </summary>
        [JsonIgnore]
        [JSIgnore]
        protected Func<IDocumentSession, IRepository<TRefObject>> RepositoryFactory { get; set; }


        public RefrenceSession<TRefObject> GetRefrenceSession()
        {
            //Гарантируем открытие сессии вне скоупов для конкретной транкации
            IDocumentSession session;
            using (var rootSession = _sessionFactory())
            {
                session = rootSession.Advanced.DocumentStore.OpenSession();
                return new RefrenceSession<TRefObject>(RepositoryFactory(session), session);
            }
        }

        /// <summary>
        ///     Ид на который ссылаемся
        /// </summary>
        [DisplayName("Идентификатор объекта")]
        [Required(ErrorMessage = "Укажите идентификатор объекта")]
        public string Id { get; set; }
    }
}