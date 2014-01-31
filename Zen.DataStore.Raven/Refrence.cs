using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Raven.Client;
using log4net;
using JSIgnore = Raven.Imports.Newtonsoft.Json.JsonIgnoreAttribute;

namespace Zen.DataStore
{
    /// <summary>
    ///     ����������������� ������ �� ������
    /// </summary>
    /// <typeparam name="TRefObject">��� �������</typeparam>
    [Serializable]
    public class Refrence<TRefObject> : IRefrence where TRefObject : class, IHasStringId
    {
        private Func<IDocumentSession> _sessionFactory;       
        public Refrence()
        {
            _sessionFactory = null;
            RepositoryFactory = null;
            SkipLoad = RefrenceHacks.SkipRefrencesByDefault;
        }

        public Refrence(Func<IDocumentSession, IRepository<TRefObject>> repository,
            Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
            RepositoryFactory = repository;
            SkipLoad = RefrenceHacks.SkipRefrencesByDefault;
        }


        private TRefObject _refObject;
        private string _id;

        /// <summary>
        /// �� �������� �������� ������
        /// </summary>
        [JsonIgnore]
        [JSIgnore]
        public bool SkipLoad { get; set; }

        /// <summary>
        ///     ������ �� ������� ���������
        /// </summary>
        [JsonIgnore]
        [JSIgnore]
        public virtual TRefObject Object
        {
            get
            {
                if (SkipLoad || RefrenceHacks.SkipRefrences)
                    return default(TRefObject);
                IAppScope scope = null;
                try
                {
                    //��� �� ���������� ������������� ������� ���������� ������
                    if (_sessionFactory == null || RepositoryFactory == null)
                    {
                        scope = AppCore.Instance.BeginScope();
                        _sessionFactory = scope.Resolve<Func<IDocumentSession>>();
                        RepositoryFactory = scope.Resolve<Func<IDocumentSession, IRepository<TRefObject>>>();
                    }

                    using (var sess = GetRefrenceSession())
                    {
                        if (_refObject == default(TRefObject) && sess.Repository != null)
                        {
                            _refObject = sess.Repository.Find(Id);
                        }

                        return _refObject;
                    }
                }
                finally
                {
                    //���� ���� ������� ���������� ������
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
                
                _refObject = default(TRefObject);
            }
        }

        /// <summary>
        ///     ����������� ��������
        /// </summary>
        [JsonIgnore]
        [JSIgnore]
        protected Func<IDocumentSession, IRepository<TRefObject>> RepositoryFactory { get; set; }


        public RefrenceSession<TRefObject> GetRefrenceSession()
        {
            // ���� _sessionFactory == null �� ������� �������� ����� � ������������������ �����, 
            // ������ ����������������� ����� ����������� ������. �� ����� ������ �������� �������� _sessionFactory
            // � �������� ��� ����� ��������� ������.
            IAppScope localScope = null;
            try
            {
                if (_sessionFactory == null || RepositoryFactory == null)
                {
                    localScope = AppCore.Instance.BeginScope();
                    _sessionFactory = localScope.Resolve<Func<IDocumentSession>>();
                    RepositoryFactory = localScope.Resolve<Func<IDocumentSession, IRepository<TRefObject>>>();
                }
                
                using (var rootSession = _sessionFactory())
                {
                    IDocumentSession session = rootSession.Advanced.DocumentStore.OpenSession();
                    IRepository<TRefObject> repository = RepositoryFactory(session);
                    
                    if (localScope != null)
                    {
                        _sessionFactory = null;
                        RepositoryFactory = null;
                    }

                    return new RefrenceSession<TRefObject>(repository, session, localScope);
                }
            }
            catch
            {
                if (localScope != null)
                    localScope.Dispose();

                throw;
            }
        }

        /// <summary>
        ///     �� �� ������� ���������
        /// </summary>
        [DisplayName("������������� �������")]
        [Required(ErrorMessage = "������� ������������� �������")]
        public string Id
        {
            get { return _id; }
            set
            {
                _id = value;
                _refObject = default(TRefObject);
            }
        }
    }
}