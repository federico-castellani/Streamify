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
        _language = cfg["TMDB:Language"] ?? "it-IT"; // default Italian
    }

    private string Q(string path, string qs)
        => $"{path}?api_key={_apiKey}&language={_language}&{qs}";

    public async Task<IReadOnlyList<TmdbSearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<TmdbSearchResult>();

        var movieTask = _http.GetFromJsonAsync<SearchMovieDto>(Q("search/movie", $"query={Uri.EscapeDataString(query)}"), ct);
        var tvTask = _http.GetFromJsonAsync<SearchTvDto>(Q("search/tv", $"query={Uri.EscapeDataString(query)}"), ct);
        await Task.WhenAll(movieTask, tvTask);

        var list = new List<TmdbSearchResult>();

        if (movieTask.Result?.results is { } mv)
            list.AddRange(mv.Select(m => new TmdbSearchResult(m.id, m.title ?? "<senza titolo>", false, m.poster_path, m.overview)));
        if (tvTask.Result?.results is { } tv)
            list.AddRange(tv.Select(s => new TmdbSearchResult(s.id, s.name ?? "<senza nome>", true, s.poster_path, s.overview)));

        return list
            .OrderByDescending(r => r.IsSeries)
            .Take(15)
            .ToList();
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

    private record SearchMovieDto(List<MovieItem> results);
    private record MovieItem(int id, string? title, string? poster_path, string? overview);
    private record SearchTvDto(List<TvItem> results);
    private record TvItem(int id, string? name, string? poster_path, string? overview);
    private record MovieDetailDto(int id, string title, int? runtime, string? release_date);
    private record SeriesDetailDto(int id, string name, int? number_of_seasons, List<SeasonItem>? seasons);
    private record SeasonItem(int id, int season_number);
    private record SeasonDetailDto(List<EpisodeItem>? episodes);
    private record EpisodeItem(int episode_number, string? name, int? runtime);
}