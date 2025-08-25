using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Streamify.Services;

namespace Streamify.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/login-action", HandleLoginAction);
        app.MapPost("/logout-action", HandleLogoutAction);
    }

    private static async Task HandleLoginAction(
        HttpContext context,
        ILoginService loginService,
        IWebHostEnvironment env,
        ILogger<Program> logger)
    {
        try
        {
            var form = await context.Request.ReadFormAsync();
            var email = form["email"].ToString().Trim();
            var password = form["password"].ToString();
            var rememberRaw = form["rememberMe"].ToString();
            var rememberMe = rememberRaw == "true" || rememberRaw == "on" || rememberRaw == "1";

            var result = await loginService.AuthenticateAsync(email, password);
            if (!result.IsSuccess)
            {
                context.Response.Redirect("/login?error=" + Uri.EscapeDataString(result.ErrorMessage));
                return;
            }

            // Create claims and sign in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new Claim(ClaimTypes.Email, result.Email),
                new Claim(ClaimTypes.Role, result.Role)
            };
            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync("Cookies", principal, new AuthenticationProperties
            {
                IsPersistent = rememberMe
            });

            logger.LogInformation("Login (remember={Remember}) for {Email} role={Role}", rememberMe, result.Email, result.Role);
            context.Response.Redirect("/?login=success");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login action error");
            context.Response.Redirect("/login?error=" + Uri.EscapeDataString("Unexpected error"));
        }
    }

    private static Task HandleLogoutAction(HttpContext context, ILogger<Program> logger)
    {
        try
        {
            context.Response.Cookies.Delete("auth_token", new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = !context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
                SameSite = SameSiteMode.Strict
            });
            logger.LogInformation("Logout");
            context.Response.Redirect("/logout?action=cleanup");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Logout error");
            context.Response.Redirect("/login?error=" + Uri.EscapeDataString("Logout error"));
        }
        return Task.CompletedTask;
    }
}