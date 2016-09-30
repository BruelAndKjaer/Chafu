namespace Chafu
{
    /// <summary>
    /// Media item
    /// </summary>
    public class MediaItem
    {
        /// <summary>
        /// Type of media
        /// </summary>
        public ChafuMediaType MediaType { get; set; }

        /// <summary>
        /// File path
        /// </summary>
        public string Path { get; set; }
    }
}