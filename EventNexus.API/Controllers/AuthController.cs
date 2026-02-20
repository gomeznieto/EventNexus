namespace EventNexus.API.Controllers;

using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase {
    
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
       _authService = authService; 
    }

    [HttpPost("register")]
    public async Task<IActionResult>Register([FromBody] RegisterRequestDtos dto){
        var response = await _authService.RegisterAsync(dto);
        return Created($"/api/users/{response}", new { Message = "Usuario registrado exitosamente.", UserId = response });
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
}
