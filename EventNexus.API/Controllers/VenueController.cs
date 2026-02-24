using System.Security.Claims;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VenueController : ControllerBase {
   private readonly IVenueService _venueService;

    public VenueController(IVenueService venueService)
    {
         _venueService = venueService;
    }

    [HttpPost("Create")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Create(CreateVenueRequestDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(userId is null) return BadRequest();

        var response = await _venueService.CreateAsync(dto, userId);

        return StatusCode(201, response);
    }
}
