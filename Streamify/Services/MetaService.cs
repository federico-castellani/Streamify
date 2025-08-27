using Streamify.TMDB;
using Streamify.Utility;
using System.Collections.Concurrent;

namespace Streamify.Services
{
    public class MetaService
    {
        private readonly ITmdbClient _tmdb;
        private readonly ConcurrentDictionary<int, Meta> _metaCache = new();

        public MetaService(ITmdbClient tmdb)
        {
            _tmdb = tmdb;
        }

        public async Task<Meta> GetMetaAsync(int tmdbId, string title, bool isSeries)
        {
            if (_metaCache.TryGetValue(tmdbId, out var meta))
                return meta;

            var results = await _tmdb.SearchAsync(title);
            var match = results.FirstOrDefault(r => r.Id == tmdbId)
                        ?? results.FirstOrDefault(r => string.Equals(r.Title, title, StringComparison.OrdinalIgnoreCase));

            meta = new Meta(tmdbId, title, isSeries, match?.PosterPath, match?.BackdropPath, match?.Overview);
            _metaCache[tmdbId] = meta;
            return meta;
        }

        public async Task LoadMetadataBatchAsync(IEnumerable<(int tmdbId, string title, bool isSeries)> items)
        {
            var tasks = items.Select(i => GetMetaAsync(i.tmdbId, i.title, i.isSeries));
            await Task.WhenAll(tasks);
        }
    }
}