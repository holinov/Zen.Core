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
        private readonly Func<IDocumentSession> _sessionFactory;

        public Refrence()
        {
            _sessionFactory = AppCore.Instance.Resolve<Func<IDocumentSession>>();
            RepositoryFactory = AppCore.Instance.Resolve<Func<IDocumentSession, IRepository<TRefObject>>>();
        }

        public Refrence(Func<IDocumentSession, IRepository<TRefObject>> repository,
            Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
            RepositoryFactory = repository;
        }

        /// <summary>
        ///     Объект на который ссылаемся
        /// </summary>
        [JsonIgnore]
        [JSIgnore]
        public virtual TRefObject Object
        {
            get
            {
                using (var sess=GetRefrenceSession())
                {
                    return !RefrenceHacks.SkipRefrences && sess.Repository!=null
                               ? sess.Repository.Find(Id)
                               : default(TRefObject);
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
            var session = _sessionFactory();
            return new RefrenceSession<TRefObject>(RepositoryFactory(session), session);
        }

        /// <summary>
        ///     Ид на который ссылаемся
        /// </summary>
        [DisplayName("Идентификатор объекта")]
        [Required(ErrorMessage = "Укажите идентификатор объекта")]
        public string Id { get; set; }
    }

    public class RefrenceSession<TRefObject>:IDisposable
    {
        private readonly IDocumentSession _session;
        private readonly IRepository<TRefObject> _repository;

        public RefrenceSession(IRepository<TRefObject> repository, IDocumentSession session)
        {
            _repository = repository;
            _session = session;
        }

        public IRepository<TRefObject> Repository
        {
            get { return _repository; }
        }

        public IDocumentSession Session
        {
            get { return _session; }
        }

        public void Dispose()
        {
            Repository.Dispose();
            Session.Dispose();
        }
    }
}