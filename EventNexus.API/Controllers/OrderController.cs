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

   [HttpPost]
   public async Task<IActionResult> CreateOrder (CreateOrderRequestDto dto){
       var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
       var userEmail = User.FindFirstValue(ClaimTypes.Email);

        if(userId is null) return BadRequest();

       var response =  await _orderService.CreateOrderAsync(dto, Guid.Parse(userId), userEmail);
       return StatusCode(201, response); //TODO:CreatAtAction
   }
}
