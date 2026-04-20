using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Dtos;
using BLA.Ordering.Application.Auth.Validators;
using BLA.Ordering.Domain.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BLA.Ordering.Web.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly RegisterUserCommandHandler _registerHandler;
    private readonly RegisterUserValidator _registerValidator;
    private readonly AuthenticateUserCommandHandler _authenticateHandler;
    private readonly AuthenticateUserValidator _authenticateValidator;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        RegisterUserCommandHandler registerHandler,
        RegisterUserValidator registerValidator,
        AuthenticateUserCommandHandler authenticateHandler,
        AuthenticateUserValidator authenticateValidator,
        ILogger<AccountController> logger)
    {
        _registerHandler = registerHandler;
        _registerValidator = registerValidator;
        _authenticateHandler = authenticateHandler;
        _authenticateValidator = authenticateValidator;
        _logger = logger;
    }

    [HttpGet("create")]
    public IActionResult Register() =>
        View("Register", new RegisterUserRequest(string.Empty, string.Empty, string.Empty));

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        [FromForm] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword)
            ModelState.AddModelError(nameof(request.ConfirmPassword), "Passwords do not match.");

        var command = new RegisterUserCommand(request.Email, request.Password);
        var validationResult = await _registerValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!ModelState.IsValid)
            return View("Register", request);

        try
        {
            await _registerHandler.HandleAsync(command, cancellationToken);
            _logger.LogInformation("Registration completed for a new user.");
            return RedirectToAction(nameof(Login));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration rejected: duplicate email attempt.");
            ModelState.AddModelError(nameof(request.Email), ex.Message);
            return View("Register", request);
        }
        catch (InvalidRegistrationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Register", request);
        }
    }

    [HttpGet("create/success")]
    public IActionResult RegisterSuccess() => View("RegisterSuccess");

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login() => View("Login", new LoginRequest(string.Empty, string.Empty));

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new AuthenticateUserCommand(request.Email, request.Password);
        var validationResult = await _authenticateValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return View("Login", request);
        }

        try
        {
            var result = await _authenticateHandler.HandleAsync(command, cancellationToken);
            await SignInCookieAsync(result, cancellationToken);
            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View("Login", request);
        }
    }

    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
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
