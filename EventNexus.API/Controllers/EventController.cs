using System.Security.Claims;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventController: ControllerBase{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpPost("create")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult>CreateEvent(CreateEventRequestDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

        if(userId is null) return BadRequest();

        var response = await _eventService.CreateAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new {id = response.Id}, response);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task <IActionResult> GetById([FromRoute] int id){
        if(id <= 0) return BadRequest();

        var response = await _eventService.GetByIdAsync(id);

        return Ok(response);
    }
}
