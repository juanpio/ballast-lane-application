using System.Security.Claims;
using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Dtos;
using BLA.Ordering.Application.Auth.Validators;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLA.Ordering.Web.Api;

[ApiController]
[ApiVersion("1.0")]
[Route("api/auth")]
public sealed class AuthApiController : ControllerBase
{
    private readonly RegisterUserCommandHandler _registerHandler;
    private readonly RegisterUserValidator _registerValidator;
    private readonly AuthenticateUserCommandHandler _authenticateHandler;
    private readonly AuthenticateUserValidator _authenticateValidator;
    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(
        RegisterUserCommandHandler registerHandler,
        RegisterUserValidator registerValidator,
        AuthenticateUserCommandHandler authenticateHandler,
        AuthenticateUserValidator authenticateValidator,
        ILogger<AuthApiController> logger)
    {
        _registerHandler = registerHandler;
        _registerValidator = registerValidator;
        _authenticateHandler = authenticateHandler;
        _authenticateValidator = authenticateValidator;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResult>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Email, request.Password);
        var validationResult = await _registerValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _registerHandler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(Register), result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticateUserResult>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AuthenticateUserCommand(request.Email, request.Password);
        var validationResult = await _authenticateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _authenticateHandler.HandleAsync(command, cancellationToken);
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
