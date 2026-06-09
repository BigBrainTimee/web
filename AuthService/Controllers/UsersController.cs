using System.Security.Claims;
using AuthService.Dtos;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/auth/users")]
public class UsersController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;

    public UsersController(IUserAuthService userAuthService)
    {
        _userAuthService = userAuthService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userAuthService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await _userAuthService.GetUserByIdAsync(id, cancellationToken);
        return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
    }

    [HttpPut("{id:int}/role")]
    public async Task<ActionResult<UserResponseDto>> UpdateRole(
        int id,
        [FromBody] UpdateUserRoleDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var adminUserId = GetUserId();
        if (adminUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var user = await _userAuthService.UpdateUserRoleAsync(adminUserId.Value, id, request, cancellationToken);
            return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var adminUserId = GetUserId();
        if (adminUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var deleted = await _userAuthService.DeleteUserAsync(adminUserId.Value, id, cancellationToken);
            return deleted ? NoContent() : NotFound(new { message = "User not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var userId) ? userId : null;
    }
}
