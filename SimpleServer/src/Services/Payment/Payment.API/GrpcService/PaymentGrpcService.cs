using System;
using Payment.API.GrpcService.Protos;

namespace Payment.API.GrpcService
{
    public class PaymentGrpcService
    {
        private readonly PaymentProtoService.PaymentProtoServiceClient _paymentProtoServiceClient;

        public PaymentGrpcService(PaymentProtoService.PaymentProtoServiceClient paymentProtoServiceClient)
        {
            _paymentProtoServiceClient = paymentProtoServiceClient ?? throw new ArgumentNullException(nameof(paymentProtoServiceClient));
        }

        public async Task<PaymentResponse> UpdateUserMembership(string userEmail, bool success)
        {
            var paymentRequest = new PaymentRequest
            {
                UserEmail = userEmail,
                IsPaymentSuccess = success
            };
            return await _paymentProtoServiceClient.UpdateUserMembershipAsync(paymentRequest);
        }

        public async Task<CustomerResponse> UpdateUserEmail(string customerEmail)
        {
            var customerRequest = new CustomerRequest
            {
                UserEmail = customerEmail,
            };
            return await _paymentProtoServiceClient.UpdateUserEmailAsync(customerRequest);
        }
    }
}
