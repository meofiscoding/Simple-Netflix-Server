using System;
using Payment.API.Entity;
using Stripe;

namespace Payment.API.Service.Stripe
{
    public interface IStripeService
    {
        Task<string> CheckOut(Subcription product, string userEmail);
        Task<Customer> GetCustomerByEmail(string email);
    }
}
