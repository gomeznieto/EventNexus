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

    [HttpPost("create")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Create(CreateVenueRequestDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(userId is null) return BadRequest();

        var response = await _venueService.CreateAsync(dto, userId);
        
        return CreatedAtAction(nameof(GetById), new {id = response.Id}, response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id){
        if(id <= 0) return BadRequest();

        var response = await _venueService.GetByIdAsync(id);

        return Ok(response);
    }
}
