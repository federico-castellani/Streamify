using Streamify.TMDB;
public interface ITmdbMetadataService
{
    string? GetPosterUrl(int tmdbId, TmdbMediaType type);
    Task<IEnumerable<TmdbSearchResult>> SearchAsync(string query, int page);
}

public enum TmdbMediaType { Movie, Tv }

public record TmdbSearchResult(int TmdbId, string Title, string? PosterUrl, DateTime? ReleaseOrAirDate, TmdbMediaType MediaType);