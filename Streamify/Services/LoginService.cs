using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Streamify;

namespace Streamify.Services;

public interface ILoginService
{
    Task<LoginResult> AuthenticateAsync(string email, string password);
    string GenerateToken(int userId, string email, string role);
    void SetAuthCookie(HttpContext context, string token, IWebHostEnvironment environment, bool rememberMe);
}

public class LoginService : ILoginService
{
    private readonly StreamifyDbContext _context;
    private readonly ILogger<LoginService> _logger;

    public LoginService(StreamifyDbContext context, ILogger<LoginService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LoginResult> AuthenticateAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return LoginResult.Failure("Email and password are required");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.HashedPassword))
            {
                _logger.LogWarning("Invalid login for {Email}", email);
                return LoginResult.Failure("Invalid email or password");
            }

            var role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role;
            _logger.LogInformation("Login ok for {Email} (ID {Id}, Role {Role})", email, user.Id, role);
            return LoginResult.Success(user.Id, user.Email, role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auth error for {Email}", email);
            return LoginResult.Failure("Authentication error");
        }
    }

    public string GenerateToken(int userId, string email, string role)
    {
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var tokenData = $"{userId}:{email}:{role}:{ts}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tokenData));
    }

    public void SetAuthCookie(HttpContext context, string token, IWebHostEnvironment environment, bool rememberMe)
    {
        var opts = new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };
        if (rememberMe)
            opts.Expires = DateTimeOffset.UtcNow.AddDays(7);

        context.Response.Cookies.Append("auth_token", token, opts);
        _logger.LogDebug("Auth cookie set (rememberMe={Remember})", rememberMe);
    }
}

public class LoginResult
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; } = string.Empty;
    public int UserId { get; }
    public string Email { get; } = string.Empty;
    public string Role { get; } = string.Empty;

    private LoginResult(bool ok, string error, int id, string email, string role)
    {
        IsSuccess = ok;
        ErrorMessage = error;
        UserId = id;
        Email = email;
        Role = role;
    }

    public static LoginResult Success(int id, string email, string role) => new(true, string.Empty, id, email, role);
    public static LoginResult Failure(string error) => new(false, error, 0, string.Empty, string.Empty);
}