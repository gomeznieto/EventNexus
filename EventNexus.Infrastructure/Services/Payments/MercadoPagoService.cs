using System.Net.Http.Json;
using EventNexus.Application.DTOs;
using EventNexus.Application.Interfaces;
using EventNexus.Domain.Entities;
using System.Text.Json;
using EventNexus.Domain.Enums;
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
        DateTime utcNow = DateTime.UtcNow;
        DateTime expirationUtc = utcNow.AddMinutes(15);
        var expirationDate = expirationUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);

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
            // payer = new {
            //     email = userEmail
            // },
            external_reference = order.Id.ToString(), 
            notification_url = "https://proverbially-explicit-delsie.ngrok-free.dev/api/webhook/mercadopago", 
            back_urls = new {
                success = "https://localhost:3000/checkout/success",
                failure = "https://localhost:3000/checkout/failure"
            },
            auto_return = "approved",
            binary_mode = true,
            expires = true,
            expiration_date_to = expirationDate
        };

        var jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = null
        };

        var response = await _httpClient.PostAsJsonAsync("checkout/preferences", preferenceRequest, jsonOptions);

        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<PreferenceResponse>();

        if (responseData == null || string.IsNullOrEmpty(responseData.SandboxInitPoint)){
            throw new Exception("Mercado Pago did not return a valid Checkout URL.");
        }

        return responseData.SandboxInitPoint;
    }

    public async Task<PaymentDetailDto> GetPaymentDetailsAsync(string paymentId)
    {
        var response = await _httpClient.GetAsync($"v1/payments/{paymentId}");

        // response.EnsureSuccessStatusCode();

        // TEST DEVELPMENT - CHANGE FOR EnsureSuccessStatusCode
        if(!response.IsSuccessStatusCode){
            var errorJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n🚨 MERCADO PAGO ERROR 🚨\n{errorJson}\n");
            throw new Exception("Mercado Pago rejected the request. Check your terminal!");
        }

        var paymentDetails = await response.Content.ReadFromJsonAsync<PaymentDetailDto>();

        return paymentDetails ?? throw new ArgumentException("Could not read payment details from Mercado Pago");
    }

}

