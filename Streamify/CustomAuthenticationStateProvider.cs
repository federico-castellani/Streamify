using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Streamify;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var ctx = _httpContextAccessor.HttpContext;
        ClaimsPrincipal principal = new(new ClaimsIdentity());

        if (ctx?.Request.Cookies.TryGetValue("auth_token", out var token) == true && !string.IsNullOrEmpty(token))
            principal = CreateUserFromToken(token);

        return Task.FromResult(new AuthenticationState(principal));
    }

    public async Task LogoutAsync()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx != null)
        {
            ctx.Response.Cookies.Delete("auth_token", new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = !ctx.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
                SameSite = SameSiteMode.Strict
            });
        }
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        await Task.CompletedTask;
    }

    private ClaimsPrincipal CreateUserFromToken(string token)
    {
        try
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':');
            // Expected: userId:email:role:ts (new) or userId:email:ts (old)
            if (parts.Length == 4)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, parts[0]),
                    new Claim(ClaimTypes.Name, parts[1]),
                    new Claim(ClaimTypes.Role, parts[2]),
                    new Claim("login_time", parts[3])
                };
                return new ClaimsPrincipal(new ClaimsIdentity(claims, "cookie"));
            }
            if (parts.Length == 3)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, parts[0]),
                    new Claim(ClaimTypes.Name, parts[1]),
                    new Claim("login_time", parts[2])
                };
                return new ClaimsPrincipal(new ClaimsIdentity(claims, "cookie"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bad token");
        }
        return new ClaimsPrincipal(new ClaimsIdentity());
    }
}