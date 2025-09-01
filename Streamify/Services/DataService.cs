using Streamify.Data;
using Microsoft.EntityFrameworkCore;

namespace Streamify.Services
{
    public class DataService
    {
        private readonly StreamifyDbContext _db;

        public DataService(StreamifyDbContext db)
        {
            _db = db;
        }

        public async Task<List<WatchHistory>> GetContinueMovieEntriesAsync(int userId)
            => await _db.WatchHistories
                .Include(h => h.Movie)
                .Where(h => h.UserId == userId && h.MovieId != null && !h.Completed && h.ProgressSeconds > 0)
                .OrderByDescending(h => h.LastWatched)
                .Take(20)
                .ToListAsync();

        public async Task<List<WatchHistory>> GetContinueEpisodeEntriesAsync(int userId)
            => await _db.WatchHistories
                .Include(h => h.Episode)!.ThenInclude(e => e.Series)
                .Where(h => h.UserId == userId && h.EpisodeId != null && !h.Completed && h.ProgressSeconds > 0)
                .OrderByDescending(h => h.LastWatched)
                .Take(20)
                .ToListAsync();

        public async Task<List<Movie>> GetPopularMoviesAsync()
            => await _db.Movies
                .OrderByDescending(m => m.WatchHistoryEntries.Count)
                .ThenBy(m => m.MovieId)
                .Take(30)
                .ToListAsync();

        public async Task<List<Series>> GetPopularSeriesAsync()
        {
            var seriesCounts = await _db.WatchHistories
                .Where(h => h.EpisodeId != null && h.Episode != null && h.Episode.SeriesId != null)
                .GroupBy(h => h.Episode!.SeriesId)
                .Select(g => new { SeriesId = g.Key, Count = g.Count() })
                .ToListAsync();
            var seriesLookup = seriesCounts
                .Where(x => x.SeriesId != null)
                .ToDictionary(x => x.SeriesId, x => x.Count);

            var allSeries = await _db.Series
                .Include(s => s.Episodes)
                .ToListAsync();

            return allSeries
                .OrderByDescending(s => seriesLookup.TryGetValue(s.SeriesId, out var c) ? c : 0)
                .ThenBy(s => s.SeriesId)
                .Take(30)
                .ToList();
        }

        public async Task<List<Movie>> GetRecentMoviesAsync()
            => await _db.Movies
                .OrderByDescending(m => m.MovieId)
                .Take(30)
                .ToListAsync();

        public async Task<List<Series>> GetRecentSeriesAsync()
            => await _db.Series
                .OrderByDescending(s => s.SeriesId)
                .Take(30)
                .ToListAsync();
        public async Task<List<Movie>> SearchMoviesAsync(string searchTerm)
            => await _db.Movies
                .Where(m => m.Title.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(m => m.Title)
                .Take(20)
                .ToListAsync();

        public async Task<List<Series>> SearchSeriesAsync(string searchTerm)
            => await _db.Series
                .Where(s => s.Title.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(s => s.Title)
                .Take(20)
                .ToListAsync();
    }
}