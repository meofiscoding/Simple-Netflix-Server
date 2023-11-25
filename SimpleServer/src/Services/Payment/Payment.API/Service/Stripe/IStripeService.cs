using System;
using Payment.API.Entity;

namespace Payment.API.Service.Stripe
{
    public interface IStripeService
    {
        Task<string> CheckOut(Subcription product, string userEmail);
    }
}
