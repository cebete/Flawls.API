using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flawls.API.Data;
using Flawls.API.DTOs;
using Flawls.API.Models;
using Flawls.API.Services;

namespace Flawls.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, TokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password." });

        return Ok(new LoginResponse(tokenService.CreateToken(user), user.Username, user.Role, user.Language));
    }

    [HttpPost("users")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> CreateUser(CreateUserRequest req)
    {
        if (await db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { message = "Username already exists." });

        db.Users.Add(new User
        {
            Username = req.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = req.Role
        });
        await db.SaveChangesAsync();
        return Ok(new { message = "User created.", req.Username, req.Role });
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult Me() => Ok(new
    {
        username = User.Identity?.Name,
        role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
    });

    [HttpGet("users")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> GetUsers()
    {
        var users = await db.Users
            .OrderBy(u => u.CreatedAt)
            .Select(u => new { u.Id, u.Username, u.Role, u.CreatedAt })
            .ToListAsync();
        return Ok(users);
    }

    [HttpDelete("users/{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        if (id == currentUserId)
            return BadRequest(new { message = "You cannot delete your own account." });

        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("language")]
    [Authorize]
    public async Task<ActionResult> UpdateLanguage([FromBody] UpdateLanguageRequest req)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        user.Language = req.Language == "tr" ? "tr" : "en";
        await db.SaveChangesAsync();
        return Ok(new { user.Language });
    }
}