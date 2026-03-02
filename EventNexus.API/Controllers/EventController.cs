using System.Security.Claims;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Constants;
using EventNexus.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventController: ControllerBase{
    private readonly IEventService _eventService;
    private readonly IVerificationCodeService _codeService;

    public EventController(
            IEventService eventService,
            IVerificationCodeService codeService
            )
    {
        _eventService = eventService;
        _codeService = codeService;
    }

    /// <summary>
    /// Creates a new event for the authenticated organizer.
    /// </summary>
    /// <remarks>
    /// Ensure the capacity is greater than 0. The event will be created in a "Draft" status by default.
    /// </remarks>
    /// <param name="dto">The JSON object containing the event details.</param>
    /// <response code="201">Returns the newly created event object.</response>
    /// <response code="400">If the input data is invalid or missing fields.</response>
    /// <response code="401">If the user is not logged in.</response>
    [HttpPost]
    [HttpPost("create")]
    [Authorize(Roles = AppRoles.Organizer)]
    public async Task<IActionResult>CreateEvent(CreateEventRequestDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

        if(userId is null) return BadRequest();

        var response = await _eventService.CreateAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new {id = response.Id}, response);
    }

    /// <summary>
    /// Sending request to update event
    /// </summary>
    [HttpPost("{id:int}/request-publish")]
    [Authorize(Roles = AppRoles.Organizer)]
    public async Task<IActionResult> RequestPublish([FromRoute] int id){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        
        if(userId is null || userEmail is null) return BadRequest();

        var eventDto = new RequestUpdateEventDto {
            UserId = Guid.Parse(userId),
            Email =  userEmail,
            EventId = id,
            Action = ActionType.PublishEvent
        };

        var response = await _eventService.RequestUpdateEventAsync(eventDto);
        return Ok(response);
    }

    /// <summary>
    /// Sending Code to update event
    /// </summary>
    [HttpPost("{id:int}/publish")]
    [Authorize(Roles = AppRoles.Organizer)]
    public async Task<IActionResult>Publish([FromRoute] int id, [FromBody] VerificationCodeDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(userId is null) return BadRequest();

        var eventDto = new VerificationEventCodeDto {
            Code = dto.Code,
            UserId = Guid.Parse(userId),
            EventId = id
        };

        var response = await _eventService.PublishEventAsync(eventDto);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task <IActionResult> GetById([FromRoute] int id){
        if(id <= 0) return BadRequest();

        var response = await _eventService.GetByIdAsync(id);

        return Ok(response);
    }

    /// <summary>
    /// Sending request to update event
    /// </summary>
    [HttpPost("{id:int}/request-cancellation")]
    [Authorize(Roles = AppRoles.Organizer)]
    public async Task <IActionResult> RequestCancellation([FromRoute] int id){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = User.FindFirstValue(ClaimTypes.Email);

        if(userId is null || userEmail is null) return BadRequest();

        var eventDto = new RequestUpdateEventDto {
            UserId = Guid.Parse(userId),
            Email =  userEmail,
            EventId = id,
            Action = ActionType.CancelEvent
        };

        var response = await _eventService.RequestUpdateEventAsync(eventDto);
        return Ok(response);
    }

    [HttpPost("{id:int}/cancellation")]
    [Authorize(Roles = AppRoles.Organizer)]
    public async Task<IActionResult>Cancellation([FromRoute] int id, [FromBody] VerificationCodeDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(userId is null) return BadRequest();

        var eventDto = new VerificationEventCodeDto {
            Code = dto.Code,
            UserId = Guid.Parse(userId),
            EventId = id
        };

        var response = await _eventService.CancelEventAsync(eventDto);
        return Ok(response);
    }

}
