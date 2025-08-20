namespace Streamify.Data;

public class Series
{
    public int SeriesId { get; set; }
    public int TmdbId { get; set; }
    public string Title { get; set; } = null!;

    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}