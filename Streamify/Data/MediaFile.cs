namespace Streamify.Data;

public class MediaFile
{
    public int MediaFileId { get; set; }
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }
    
    public string FilePath { get; set; } = null!;

    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}