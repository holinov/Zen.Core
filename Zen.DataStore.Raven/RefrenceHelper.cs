using System.Linq;
using System.Text;
using log4net;

namespace Zen.DataStore
{
    public static class RefrenceHelpers
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (RefrenceHelpers));
        /// <summary>
        /// Установить на бизнес сущности загружить или нет ссылку
        /// </summary>
        /// <typeparam name="T">Тип сущности</typeparam>
        /// <param name="obj">Сущность</param>
        /// <param name="skip">Не загружать если TRUE</param>
        /// <returns>Сущность</returns>
        public static T SkipRefrences<T>(this T obj,bool skip) where T : IHasStringId
        {
            var sb = new StringBuilder();
            sb.AppendLine("Установка загрузки ссылок в " + skip);
            //TODO: Переделать на кешированные експрешны
            var type = typeof (T);
            foreach (var prop in type.GetProperties().Where(p=>p.PropertyType==typeof(Refrence<>)))
            {
                //Конкретный тип ссылки
                var pType = prop.PropertyType;

                //Объект значение ссылки конкретного типа
                var pVal = prop.GetValue(obj);

                //Свойство разрешающее загрузку значения ссылки
                var loadProp = pType.GetProperty("SkipLoad");
                sb.AppendLine("Установлено для свойства " + prop.Name);
                loadProp.SetValue(pVal, skip);
            }
            Log.Debug(sb);
            return obj;
        }

        /// <summary>
        /// Пропускать загрузку ссылок
        /// </summary>
        /// <typeparam name="T">Тип сущности</typeparam>
        /// <param name="obj">Сущность</param>
        /// <returns>Сущность</returns>
        public static T SkipRefrences<T>(this T obj) where T : IHasStringId
        {
            return SkipRefrences(obj, true);
        }

        /// <summary>
        /// Загружать ссылки
        /// </summary>
        /// <typeparam name="T">Тип сущности</typeparam>
        /// <param name="obj">Сущность</param>
        /// <returns>Сущность</returns>
        public static T LoadRefrences<T>(this T obj) where T : IHasStringId
        {
            return SkipRefrences(obj, false);
        }
    }
}