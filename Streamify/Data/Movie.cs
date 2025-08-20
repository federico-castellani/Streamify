namespace Streamify.Data;

public class Movie
{
    public int MovieId { get; set; }
    public int TmdbId { get; set; }
    public string Title { get; set; } = null!;
    public int? DurationMinutes { get; set; }
    public DateTime? ReleaseDate { get; set; }

    public ICollection<WatchHistory> WatchHistoryEntries { get; set; } = new List<WatchHistory>();
    public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
}