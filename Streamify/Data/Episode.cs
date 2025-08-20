namespace Streamify.Data;

public class Episode
{
    public int EpisodeId { get; set; }
    public int SeriesId { get; set; }

    public int SeasonNumber { get; set; }
    public int EpisodeNumber { get; set; }
    public string Title { get; set; } = null!;
    public int? DurationMinutes { get; set; }

    public Series Series { get; set; } = null!;
    public MediaFile? MediaFile { get; set; }
}