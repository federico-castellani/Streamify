using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Streamify.TMDB;

// Enhanced interface with image URL generation
public interface ITmdbClient
{
    Task<IReadOnlyList<TmdbSearchResult>> SearchAsync(string query, CancellationToken ct = default);
    Task<TmdbMovieDetail?> GetMovieAsync(int id, CancellationToken ct = default);
    Task<TmdbSeriesDetail?> GetSeriesAsync(int id, CancellationToken ct = default);
    Task<TmdbSeasonDetail?> GetSeasonAsync(int seriesId, int seasonNumber, CancellationToken ct = default);
    
    // New image URL generation methods
    string? GetImageUrl(string? imagePath, TmdbImageType imageType, TmdbImageSize size = TmdbImageSize.Original);
    string? GetPosterUrl(string? posterPath, TmdbImageSize size = TmdbImageSize.W342);
    string? GetBackdropUrl(string? backdropPath, TmdbImageSize size = TmdbImageSize.W780);
    string? GetLogoUrl(string? logoPath, TmdbImageSize size = TmdbImageSize.W185);
    string? GetProfileUrl(string? profilePath, TmdbImageSize size = TmdbImageSize.W185);
    string? GetStillUrl(string? stillPath, TmdbImageSize size = TmdbImageSize.W300);
}

// Image type enumeration
public enum TmdbImageType
{
    Poster,
    Backdrop,
    Logo,
    Profile,
    Still
}

// Image size enumeration with all TMDB supported sizes
public enum TmdbImageSize
{
    // Poster sizes
    W92,
    W154,
    W185,
    W342,
    W500,
    W780,
    
    // Backdrop sizes
    W300,
    W1280,
    
    // Logo sizes
    W45,
    
    // Profile sizes
    H632,
    
    // Universal
    Original
}

// Enhanced records with additional image paths
public record TmdbSearchResult(
    int Id, 
    string Title, 
    bool IsSeries, 
    string? PosterPath, 
    string? BackdropPath,
    string? Overview);

public record TmdbMovieDetail(
    int Id, 
    string Title, 
    int? Runtime, 
    DateTime? ReleaseDate,
    string? PosterPath,
    string? BackdropPath);

public record TmdbSeriesDetail(
    int Id, 
    string Name, 
    int? NumberOfSeasons, 
    IReadOnlyList<int> SeasonNumbers,
    string? PosterPath,
    string? BackdropPath);

public record TmdbSeasonDetail(
    int SeriesId, 
    int SeasonNumber, 
    IReadOnlyList<TmdbEpisodeInfo> Episodes,
    string? PosterPath);

public record TmdbEpisodeInfo(
    int EpisodeNumber, 
    string Name, 
    int? Runtime,
    string? StillPath);
    
public static class TmdbImageHelper
{
    private const string BaseImageUrl = "https://image.tmdb.org/t/p/";
    
    // Size mappings for each image type
    private static readonly Dictionary<TmdbImageType, HashSet<TmdbImageSize>> ValidSizes = new()
    {
        [TmdbImageType.Poster] = new HashSet<TmdbImageSize> 
        { 
            TmdbImageSize.W92, TmdbImageSize.W154, TmdbImageSize.W185, 
            TmdbImageSize.W342, TmdbImageSize.W500, TmdbImageSize.W780, 
            TmdbImageSize.Original 
        },
        [TmdbImageType.Backdrop] = new HashSet<TmdbImageSize> 
        { 
            TmdbImageSize.W300, TmdbImageSize.W780, TmdbImageSize.W1280, 
            TmdbImageSize.Original 
        },
        [TmdbImageType.Logo] = new HashSet<TmdbImageSize> 
        { 
            TmdbImageSize.W45, TmdbImageSize.W92, TmdbImageSize.W154, 
            TmdbImageSize.W185, TmdbImageSize.W300, TmdbImageSize.W500, 
            TmdbImageSize.Original 
        },
        [TmdbImageType.Profile] = new HashSet<TmdbImageSize> 
        { 
            TmdbImageSize.W45, TmdbImageSize.W185, TmdbImageSize.H632, 
            TmdbImageSize.Original 
        },
        [TmdbImageType.Still] = new HashSet<TmdbImageSize> 
        { 
            TmdbImageSize.W92, TmdbImageSize.W185, TmdbImageSize.W300, 
            TmdbImageSize.Original 
        }
    };

    // Default sizes for each image type
    private static readonly Dictionary<TmdbImageType, TmdbImageSize> DefaultSizes = new()
    {
        [TmdbImageType.Poster] = TmdbImageSize.W342,
        [TmdbImageType.Backdrop] = TmdbImageSize.W780,
        [TmdbImageType.Logo] = TmdbImageSize.W185,
        [TmdbImageType.Profile] = TmdbImageSize.W185,
        [TmdbImageType.Still] = TmdbImageSize.W300
    };

    public static string? BuildImageUrl(string? imagePath, TmdbImageType imageType, TmdbImageSize size = TmdbImageSize.Original)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return null;

        // Ensure the path starts with a forward slash
        if (!imagePath.StartsWith("/"))
            imagePath = "/" + imagePath;

        // Use default size if Original is specified or size is not valid for the image type
        var actualSize = size;
        if (size == TmdbImageSize.Original || !ValidSizes[imageType].Contains(size))
        {
            actualSize = size == TmdbImageSize.Original ? TmdbImageSize.Original : DefaultSizes[imageType];
        }

        var sizeString = GetSizeString(actualSize);
        return $"{BaseImageUrl}{sizeString}{imagePath}";
    }

    public static string GetSizeString(TmdbImageSize size) => size switch
    {
        TmdbImageSize.W45 => "w45",
        TmdbImageSize.W92 => "w92",
        TmdbImageSize.W154 => "w154",
        TmdbImageSize.W185 => "w185",
        TmdbImageSize.W300 => "w300",
        TmdbImageSize.W342 => "w342",
        TmdbImageSize.W500 => "w500",
        TmdbImageSize.W780 => "w780",
        TmdbImageSize.W1280 => "w1280",
        TmdbImageSize.H632 => "h632",
        TmdbImageSize.Original => "original",
        _ => "original"
    };

    public static TmdbImageSize GetBestSizeForWidth(TmdbImageType imageType, int targetWidth)
    {
        var validSizes = ValidSizes[imageType].Where(s => s != TmdbImageSize.Original).ToList();
        
        return imageType switch
        {
            TmdbImageType.Poster => targetWidth switch
            {
                <= 92 => TmdbImageSize.W92,
                <= 154 => TmdbImageSize.W154,
                <= 185 => TmdbImageSize.W185,
                <= 342 => TmdbImageSize.W342,
                <= 500 => TmdbImageSize.W500,
                <= 780 => TmdbImageSize.W780,
                _ => TmdbImageSize.Original
            },
            TmdbImageType.Backdrop => targetWidth switch
            {
                <= 300 => TmdbImageSize.W300,
                <= 780 => TmdbImageSize.W780,
                <= 1280 => TmdbImageSize.W1280,
                _ => TmdbImageSize.Original
            },
            TmdbImageType.Logo => targetWidth switch
            {
                <= 45 => TmdbImageSize.W45,
                <= 92 => TmdbImageSize.W92,
                <= 154 => TmdbImageSize.W154,
                <= 185 => TmdbImageSize.W185,
                <= 300 => TmdbImageSize.W300,
                <= 500 => TmdbImageSize.W500,
                _ => TmdbImageSize.Original
            },
            TmdbImageType.Profile => targetWidth switch
            {
                <= 45 => TmdbImageSize.W45,
                <= 185 => TmdbImageSize.W185,
                _ => TmdbImageSize.H632
            },
            TmdbImageType.Still => targetWidth switch
            {
                <= 92 => TmdbImageSize.W92,
                <= 185 => TmdbImageSize.W185,
                <= 300 => TmdbImageSize.W300,
                _ => TmdbImageSize.Original
            },
            _ => TmdbImageSize.Original
        };
    }

    public static bool IsValidSizeForImageType(TmdbImageType imageType, TmdbImageSize size)
    {
        return ValidSizes[imageType].Contains(size);
    }

    public static IEnumerable<TmdbImageSize> GetValidSizesForImageType(TmdbImageType imageType)
    {
        return ValidSizes[imageType];
    }
}