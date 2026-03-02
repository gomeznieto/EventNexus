using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;

namespace EventNexus.Infrastructure.Services;

public class MercadoPagoService : IMercadoPagoService
{
    private readonly HttpClient _httpClient;
    public MercadoPagoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> CreatePaymentPreferenceAsync(Order order, string userEmail)
    {
        var expirationDate = order.ExpiresAt.ToString("yyyy-MM-ddTHH:mm:ss.fffK");

        var preferenceRequest = new
        {
            items = new[]
            {
                new {
                    title = "Event Tickets",
                    quantity = order.Tickets.Count,
                    unit_price = order.TotalAmount / order.Tickets.Count,
                    currency_id = "ARS"
                }
            },
            payer = new {
                email = userEmail
            },
            external_reference = order.Id.ToString(), 

            // Webhook for your backend to listen to
            notification_url = "https://your-ngrok-url.app/api/webhooks/mercadopago", 

            // Redirects for the frontend
            back_urls = new {
                success = "http://localhost:3000/checkout/success",
                failure = "http://localhost:3000/checkout/failure"
            },
            auto_return = "approved",

            binary_mode = true,
            expires = true,
            expiration_date_to = expirationDate
        };

        var response = await _httpClient.PostAsJsonAsync("checkout/preferences", preferenceRequest);
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<PreferenceResponse>();

        if (responseData == null || string.IsNullOrEmpty(responseData.SandboxInitPoint))
        {
            throw new Exception("Mercado Pago did not return a valid Checkout URL.");
        }

        return responseData.SandboxInitPoint;
    }
}

