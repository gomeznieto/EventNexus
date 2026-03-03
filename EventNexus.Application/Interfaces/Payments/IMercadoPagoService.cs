using EventNexus.Application.DTOs;
using EventNexus.Domain.Entities;

namespace EventNexus.Application.Interfaces;

public interface IMercadoPagoService {
    /// <summary>
    /// Sends the order details to Mercado Pago and returns the checkout URL.
    /// </summary>
    /// <param name="order">The saved order entity.</param>
    /// <returns>A string containing the URL where the user can pay.</returns>
    Task <string> CreatePaymentPreferenceAsync(Order order, string userEmail);
    Task<PaymentDetailDto> GetPaymentDetailsAsync(string paymentId);
}

