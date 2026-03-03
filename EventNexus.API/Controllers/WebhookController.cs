using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EventNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase {

    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly IOrderService _orderService;
    public WebhookController(
            IMercadoPagoService mercadoPagoService,
            IOrderService orderService
            )
    {
        _mercadoPagoService = mercadoPagoService;
        _orderService = orderService;
    }

    [HttpPost("mercadopago")]
    public async Task<IActionResult> MercadoPagoWebhook([FromBody] MercadoPagoWebhookDto payload){
        if(payload is null || payload.Action != "payment.created")
            return Ok();

        var paymentDetails = await _mercadoPagoService.GetPaymentDetailsAsync(payload.Data.Id);

        if(paymentDetails.Status == PaymentStatus.Approved) 
            await _orderService.FullFillOrderAync(paymentDetails.ExternalReference);

        return Ok();
    }

}
