using System.Threading;
using System.Threading.Tasks;

namespace Streamify.TMDB;

public interface ITmdbClient
{
    Task<IReadOnlyList<TmdbSearchResult>> SearchAsync(string query, CancellationToken ct = default);
    Task<TmdbMovieDetail?> GetMovieAsync(int id, CancellationToken ct = default);
    Task<TmdbSeriesDetail?> GetSeriesAsync(int id, CancellationToken ct = default);
    Task<TmdbSeasonDetail?> GetSeasonAsync(int seriesId, int seasonNumber, CancellationToken ct = default);
}

public record TmdbSearchResult(int Id, string Title, bool IsSeries, string? PosterPath, string? Overview);
public record TmdbMovieDetail(int Id, string Title, int? Runtime, DateTime? ReleaseDate);
public record TmdbSeriesDetail(int Id, string Name, int? NumberOfSeasons, IReadOnlyList<int> SeasonNumbers);
public record TmdbSeasonDetail(int SeriesId, int SeasonNumber, IReadOnlyList<TmdbEpisodeInfo> Episodes);
public record TmdbEpisodeInfo(int EpisodeNumber, string Name, int? Runtime);