using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zen.DataStore
{
    /// <summary>
    ///     Денормализованная ссылка на объект
    /// </summary>
    /// <typeparam name="TRefObject">Тип объекта</typeparam>
    [Serializable]
    public class Refrence<TRefObject> : IRefrence where TRefObject : class, IHasStringId
    {
        public Refrence()
        {
        }

        public Refrence(IRepository<TRefObject> repository)
        {
            Repository = repository;
        }

        /// <summary>
        ///     Объект на который ссылаемся
        /// </summary>
        [JsonIgnore]
        //[Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public virtual TRefObject Object
        {
            get
            {
                return Repository != null && !RefrenceHacks.SkipRefrences
                           ? Repository.Find(Id)
                           : default(TRefObject);
            }
            set
            {
                if (value != null)
                {
                    Id = value.Id;
                    if (Repository != null && Repository.Find(Id) == null)
                    {
                        Repository.Store(value);
                        Repository.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        ///     Репозитарий объектов
        /// </summary>
        [JsonIgnore]
        //[Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public virtual IRepository<TRefObject> Repository { get; set; }

        /// <summary>
        ///     Ид на который ссылаемся
        /// </summary>
        [DisplayName("Идентификатор объекта")]
        [Required(ErrorMessage = "Укажите идентификатор объекта")]
        public string Id { get; set; }
    }
}