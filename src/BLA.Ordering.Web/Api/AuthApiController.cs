using System.Security.Claims;
using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Dtos;
using BLA.Ordering.Application.Auth.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLA.Ordering.Web.Api;

[ApiController]
[Route("api/auth")]
public sealed class AuthApiController : ControllerBase
{
    private readonly AuthenticateUserCommandHandler _handler;
    private readonly AuthenticateUserValidator _validator;
    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(
        AuthenticateUserCommandHandler handler,
        AuthenticateUserValidator validator,
        ILogger<AuthApiController> logger)
    {
        _handler = handler;
        _validator = validator;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticateUserResult>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AuthenticateUserCommand(request.Email, request.Password);
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _handler.HandleAsync(command, cancellationToken);
            await SignInCookieAsync(result, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Login failed due to invalid credentials.");
            return Unauthorized(new { message = "Invalid email or password." });
        }
    }

    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    private async Task SignInCookieAsync(AuthenticateUserResult result, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
            new(ClaimTypes.Name, result.Email),
            new(ClaimTypes.Email, result.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = result.ExpiresAtUtc
            });
    }
}
