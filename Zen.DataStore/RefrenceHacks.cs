namespace Zen.DataStore
{
    /// <summary>
    /// Внимание! Использование этого функционала может вызвать "странные" поведения
    /// </summary>
    public static class RefrenceHacks
    {
        /// <summary>
        /// Не загружать значения ссылок
        /// </summary>
        public static bool SkipRefrences { get; set; }

        /// <summary>
        /// По умолчанию не загружать значения ленивых ссылок
        /// </summary>
        public static bool SkipRefrencesByDefault { get; set; }
    }
}