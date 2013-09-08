namespace Zen.DataStore
{
    /// <summary>
    ///     Объект с числовым идентификатором
    /// </summary>
    public interface IHasIntId : IHasSegmentId
    {
        /// <summary>
        ///     Ид записи
        /// </summary>
        int Id { get; set; }
    }
}