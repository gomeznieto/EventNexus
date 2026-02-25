using System.Security.Claims;
using EventNexus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventNexus.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase{

    private readonly IProfileService _profileService;
    public ProfileController(
            IProfileService profileService
            )
    {
        _profileService = profileService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser([FromRoute] Guid id){
        var response = await _profileService.GetByIdAsync(id);
        return Ok(response);
    }

    [HttpGet("me")]
    public async Task <IActionResult> CurrentUser(){
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null) return BadRequest();
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        var response = await _profileService.GetCurrentAsync(currentUserId, roles);
        return Ok(response);
    }
} 


