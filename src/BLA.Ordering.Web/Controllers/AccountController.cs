using BLA.Ordering.Application.Auth.Commands;
using BLA.Ordering.Application.Auth.Dtos;
using BLA.Ordering.Application.Auth.Validators;
using BLA.Ordering.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BLA.Ordering.Web.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly RegisterUserCommandHandler _handler;
    private readonly RegisterUserValidator _validator;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        RegisterUserCommandHandler handler,
        RegisterUserValidator validator,
        ILogger<AccountController> logger)
    {
        _handler = handler;
        _validator = validator;
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
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!ModelState.IsValid)
            return View("Register", request);

        try
        {
            await _handler.HandleAsync(command, cancellationToken);
            _logger.LogInformation("Registration completed for a new user.");
            return RedirectToAction(nameof(RegisterSuccess));
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
}
