using System.Security.Claims;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Enums;
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

    ///<summary>
    /// Get the user public profile.
    ///</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser([FromRoute] Guid id){
        var response = await _profileService.GetByIdAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// Get the user private profile. The auth is taken from Token
    /// </summary>
    [HttpGet("me")]
    public async Task <IActionResult> CurrentUser(){
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null) return BadRequest();
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        var response = await _profileService.GetCurrentAsync(currentUserId, roles);
        return Ok(response);
    }

    [HttpPost("change-email/request")]
    public async Task<IActionResult> RequestChangeEmail(){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        if(userId is null || userEmail is null) return BadRequest();
        
        var response = await _profileService.RequestChangeEmailAsync(new RequestUpdateDto{Email = userEmail, UserId = Guid.Parse(userId), Action = ActionType.UpdateEmail});

        return Ok(response);
    }

    [HttpPost("change-email/authorize")] 
    public async Task <IActionResult> AuthorizeEmailChange(AuthorizeEmailChangeDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        if(userId is null || userEmail is null) return BadRequest();

        var response = await _profileService.AuthorizeEmailChangeAsync(Guid.Parse(userId), userEmail, dto);

        return Ok(response);
    }

    [HttpPost("change-email/confirm")]
    public async Task<IActionResult> ConfirmEmailChange(VerificationCodeDto dto) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(userId is null) return BadRequest();

        var response = await _profileService.ConfirmEmailChangeAsync(Guid.Parse(userId), dto);

        return Ok(response);
    }
} 


