using System.Configuration;

namespace Zen
{
    /// <summary>
    /// Класс зависимость для работы с конфигурационными файлами
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Получить секцию конфигурации приложения
        /// </summary>
        /// <typeparam name="T">Тип секции</typeparam>
        /// <returns>Содержимое файла конфигурации</returns>
        public T GetConfigSection<T>() where T : class, new()
        {
            var res = (T) ConfigurationManager.GetSection(typeof (T).Name) ?? new T();
            return res;
        }

        /// <summary>
        /// Получить строку из appSettings
        /// </summary>
        /// <param name="name">Ключ строки</param>
        /// <returns>Значение</returns>
        public string GetAppSettingString(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}