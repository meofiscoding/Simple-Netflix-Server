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

        public async Task<PaymentResponse> UpdateUserMembership(string userId, bool success){
            var paymentRequest = new PaymentRequest
            {
                UserId = userId,
                Success = success
            };
            return await _paymentProtoServiceClient.UpdateUserMembershipAsync(paymentRequest);
        }
    }
}
