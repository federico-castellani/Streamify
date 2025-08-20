using Streamify;

namespace Streamify.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public interface IThemeService
{
    Task<bool> GetUserDarkModePreferenceAsync();
    Task<bool> SetUserDarkModePreferenceAsync(bool isDarkMode);
    event Action<bool>? ThemeChanged;
}

public class ThemeService : IThemeService
{
    private readonly StreamifyDbContext _context;
    private readonly AuthenticationStateProvider _authProvider;
    private readonly ILogger<ThemeService> _logger;
    private bool _currentDarkMode = false;
    private bool _isInitialized = false;

    public event Action<bool>? ThemeChanged;

    public ThemeService(
        StreamifyDbContext context, 
        AuthenticationStateProvider authProvider,
        ILogger<ThemeService> logger)
    {
        _context = context;
        _authProvider = authProvider;
        _logger = logger;
    }

    public async Task<bool> GetUserDarkModePreferenceAsync()
    {
        try
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            
            if (!authState.User.Identity?.IsAuthenticated == true)
            {
                // Default to light mode for non-authenticated users
                return false;
            }

            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Could not parse user ID from claims");
                return false;
            }

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                _currentDarkMode = user.DarkMode;
                _isInitialized = true;
                return user.DarkMode;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user dark mode preference");
            return false;
        }
    }

    public async Task<bool> SetUserDarkModePreferenceAsync(bool isDarkMode)
    {
        try
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            
            if (!authState.User.Identity?.IsAuthenticated == true)
            {
                return false;
            }

            var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                _logger.LogWarning("Could not parse user ID from claims");
                return false;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return false;
            }

            user.DarkMode = isDarkMode;
            await _context.SaveChangesAsync();

            _currentDarkMode = isDarkMode;
            
            // Notify subscribers of theme change
            ThemeChanged?.Invoke(isDarkMode);
            
            _logger.LogInformation("Updated dark mode preference for user {UserId}: {DarkMode}", userId, isDarkMode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user dark mode preference");
            return false;
        }
    }

    // Helper method to get current theme without database call
    public bool GetCurrentTheme()
    {
        return _currentDarkMode;
    }

    // Check if theme has been loaded
    public bool IsInitialized => _isInitialized;
}