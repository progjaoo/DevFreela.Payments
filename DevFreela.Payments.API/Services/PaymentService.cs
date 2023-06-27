using DevFreela.Payments.API.Models;
namespace DevFreela.Payments.API.Services
{
    public class PaymentService : IPaymentService
    {
        public Task<bool> Process(PaymentInfoInputModel paymentInfoInputModel)
        {
            /*ainda nao implementei o gateway, apenas para dizer que o pagamento foi realizado com sucesso!*/
            return Task.FromResult(true);
        }
    }
}
