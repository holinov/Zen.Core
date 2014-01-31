namespace Zen.Host
{
    public interface IHostedApp
    {
        /// <summary>
        /// Запускает процесс исполнения приложения
        /// </summary>
        void Start();

        /// <summary>
        /// Останавливает исполнение приложения
        /// </summary>
        void Stop();

        /// <summary>
        /// Область видимости приложения
        /// </summary>
        IAppScope AppScope { get; set; }
    }
}