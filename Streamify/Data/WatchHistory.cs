namespace Streamify.Data;

public class WatchHistory
{
    public int HistoryId { get; set; }
    public int UserId { get; set; }
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }

    public int ProgressSeconds { get; set; }
    public bool Completed { get; set; }
    public DateTime LastWatched { get; set; }

    public User User { get; set; } = null!;
    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}