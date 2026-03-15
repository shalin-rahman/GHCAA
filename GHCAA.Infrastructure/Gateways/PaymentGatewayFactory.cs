using GHCAA.Application.Interfaces;
using GHCAA.Domain;

namespace GHCAA.Infrastructure.Gateways
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IEnumerable<IPaymentGatewayService> _gateways;

        public PaymentGatewayFactory(IEnumerable<IPaymentGatewayService> gateways)
        {
            _gateways = gateways;
        }

        public IPaymentGatewayService GetGateway(Enums.PaymentGateway gateway)
        {
            var service = _gateways.FirstOrDefault(g => g.GatewayType == gateway);
            if (service == null)
            {
                throw new NotSupportedException($"Payment gateway {gateway} is not implemented or registered.");
            }
            return service;
        }
    }
}
