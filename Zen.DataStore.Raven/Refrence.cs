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
        }

        public Refrence(Func<IDocumentSession, IRepository<TRefObject>> repository,
            Func<IDocumentSession> sessionFactory)
        {
            _sessionFactory = sessionFactory;
            Repository = repository;
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
                using (var sess = _sessionFactory())
                using (var repos = Repository(sess))
                {
                    return !RefrenceHacks.SkipRefrences
                               ? repos.Find(Id)
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
        public virtual Func<IDocumentSession, IRepository<TRefObject>> Repository { get; set; }

        /// <summary>
        ///     Ид на который ссылаемся
        /// </summary>
        [DisplayName("Идентификатор объекта")]
        [Required(ErrorMessage = "Укажите идентификатор объекта")]
        public string Id { get; set; }
    }
}