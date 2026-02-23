namespace EventNexus.API.Controllers;

using System.Security.Claims;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
       _authService = authService; 
    }

    [HttpPost("register-customer")]
    public async Task<IActionResult>RegisterCustomer([FromBody] RegisterCustomerRequestDto dto){
        var response = await _authService.RegisterCustomerAsync(dto);
        return Created($"/api/users/{response}", new { Message = "The User is succefully created.", UserId = response });
    }

    [HttpPost("register-organizer")]
    public async Task<IActionResult>RegisterOrganizer([FromBody] RegisterOrganizerRequestDto dto){
        var response = await _authService.RegisterOrganizerAsync(dto);
        return Created($"/api/users/{response}", new { Message = "The User is succefully created.", UserId = response });
    }


    [HttpPost("login")]
    public async Task<IActionResult>Login([FromBody] LoginRequestDto dto){
        var response = await _authService.LoginAsync(dto);
        return Ok(response);
    }

    [HttpPost("verify")]
    public async Task<IActionResult>Verify([FromBody] VerifyRequestDto dto){
        var response = await _authService.VerifyLoginCodeAsync(dto);
        return Ok(response);
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken(){
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(currentUserId is null) return BadRequest();

        await _authService.RevokeTokenAsync(currentUserId);
        
        return NoContent();
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult>RefreshToken(RefreshTokenRequestDto dto){
       var response = await _authService.RefreshTokenAsync(dto); 
       return Ok(response);
    }
}
