using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Streamify.TMDB;

public class TmdbClient : ITmdbClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _language;

    public TmdbClient(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _apiKey = cfg["TMDB:ApiKey"] ?? throw new InvalidOperationException("TMDB:ApiKey missing");
        _language = cfg["TMDB:Language"] ?? "it-IT";
    }

    private string Q(string path, string qs)
        => $"{path}?api_key={_apiKey}&language={_language}&{qs}";

    public async Task<IReadOnlyList<TmdbSearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<TmdbSearchResult>();

        var dto = await _http.GetFromJsonAsync<MultiSearchDto>(Q("search/multi", $"query={Uri.EscapeDataString(query)}"), ct);
        if (dto?.results == null)
            return Array.Empty<TmdbSearchResult>();

        var raw = dto.results
            .Where(r => r.media_type == "movie" || r.media_type == "tv")
            .Select(r => new
            {
                Result = new TmdbSearchResult(
                    r.id,
                    r.media_type == "tv" ? (r.name ?? "<senza nome>") : (r.title ?? "<senza titolo>"),
                    r.media_type == "tv",
                    r.poster_path,
                    r.overview
                ),
                r.popularity
            })
            .ToList();

        var q = query.Trim().ToLowerInvariant();
        var ranked = raw
            .Select(x =>
            {
                var titleLower = x.Result.Title.ToLowerInvariant();
                double score = x.popularity;
                if (titleLower == q) score += 10_000;
                else if (titleLower.StartsWith(q)) score += 5_000;
                else if (titleLower.Contains(q)) score += 1_000;
                score += 100 - Math.Min(100, Math.Abs(titleLower.Length - q.Length));
                return (x.Result, score);
            })
            .OrderByDescending(t => t.score)
            .ThenBy(t => t.Result.Title.Length)
            .Select(t => t.Result)
            .Take(15)
            .ToList();

        return ranked;
    }

    public async Task<TmdbMovieDetail?> GetMovieAsync(int id, CancellationToken ct = default)
    {
        var dto = await _http.GetFromJsonAsync<MovieDetailDto>(Q($"movie/{id}", ""), ct);
        if (dto == null) return null;
        DateTime? rel = null;
        if (DateTime.TryParse(dto.release_date, out var d)) rel = d;
        return new TmdbMovieDetail(dto.id, dto.title, dto.runtime, rel);
    }

    public async Task<TmdbSeriesDetail?> GetSeriesAsync(int id, CancellationToken ct = default)
    {
        var dto = await _http.GetFromJsonAsync<SeriesDetailDto>(Q($"tv/{id}", ""), ct);
        if (dto == null) return null;
        var seasonNums = dto.seasons?
            .Where(s => s.season_number >= 1)
            .Select(s => s.season_number)
            .OrderBy(n => n)
            .ToList() ?? new List<int>();
        return new TmdbSeriesDetail(dto.id, dto.name, dto.number_of_seasons, seasonNums);
    }

    public async Task<TmdbSeasonDetail?> GetSeasonAsync(int seriesId, int seasonNumber, CancellationToken ct = default)
    {
        var dto = await _http.GetFromJsonAsync<SeasonDetailDto>(Q($"tv/{seriesId}/season/{seasonNumber}", ""), ct);
        if (dto == null) return null;
        var eps = dto.episodes?
            .Select(e => new TmdbEpisodeInfo(e.episode_number, e.name ?? $"Episodio {e.episode_number}", e.runtime))
            .OrderBy(e => e.EpisodeNumber)
            .ToList() ?? new List<TmdbEpisodeInfo>();
        return new TmdbSeasonDetail(seriesId, seasonNumber, eps);
    }

    private record MultiSearchDto(List<MultiItem> results);

    private record MultiItem(int id, string media_type, string? title, string? name, string? poster_path, string? overview, double popularity);

    private record MovieDetailDto(int id, string title, int? runtime, string? release_date);
    private record SeriesDetailDto(int id, string name, int? number_of_seasons, List<SeasonItem>? seasons);
    private record SeasonItem(int id, int season_number);
    private record SeasonDetailDto(List<EpisodeItem>? episodes);
    private record EpisodeItem(int episode_number, string? name, int? runtime);
}