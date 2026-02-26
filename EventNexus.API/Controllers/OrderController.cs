using System.Security.Claims;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase {
    private readonly IOrderService _orderService;
   public OrderController(IOrderService orderService)
   {
       _orderService = orderService;
   } 

   [HttpPost("create-order")]
   public async Task<IActionResult> CreateOrder (CreateOrderRequestDto dto){
       var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(userId is null) return BadRequest();

       var response =  await _orderService.CreateOrderAsync(dto, Guid.Parse(userId));
       return StatusCode(201, response); //TODO:CreatAtAction
   }
}
