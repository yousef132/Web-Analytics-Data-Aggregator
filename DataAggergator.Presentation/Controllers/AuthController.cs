using System.Security.Claims;
using DataAggergator.Application.Abstractions.Services;
using DataAggergator.Application.Dtos;
using DataAggergator.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataAggergator.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IKeycloakService _keycloakService;
    private readonly IUserService _userService;

    public AuthController(IKeycloakService keycloakService, IUserService userService)
    {
        _keycloakService = keycloakService;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Create user in Keycloak
            var keycloakUserId = await _keycloakService.CreateUserAsync(
                request.Email,
                request.Password,
                request.Name
            );

            // Create user in local database
            var user = new User
            {
                KeycloakUserId = keycloakUserId,
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            await _userService.CreateUserAsync(user);

            return Ok(new { message = "User registered successfully", userId = keycloakUserId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var tokenResponse = await _keycloakService.LoginAsync(request.Email, request.Password);

            // Update last login
            await _userService.UpdateLastLoginAsync(request.Email);

            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var tokenResponse = await _keycloakService.RefreshTokenAsync(request.RefreshToken);
            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(keycloakId))
            return Unauthorized();

        var user = await _userService.GetUserByKeycloakIdAsync(keycloakId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(keycloakId))
        {
            await _keycloakService.LogoutAsync(keycloakId);
        }

        return Ok(new { message = "Logged out successfully" });
    }
}

