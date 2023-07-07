using System.Text;
using System.Text.Json;
using DevFreela.Payments.API.Models;
using DevFreela.Payments.API.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DevFreela.Payments.API.Consumers
{
    public class ProcessPaymentConsumer : BackgroundService
    {
        private const string QUEUE = "Payments";
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public ProcessPaymentConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            //criar o factory para publicar a mensagem
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            //cria conexão
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            //declara a fila
            _channel.QueueDeclare(
                queue: QUEUE,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        //metodo que processa a mensagem da fila declarada acima...
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //instanciar uma classe eventingBasic e receber um canal
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, eventArgs) =>
            {
                var byteArray = eventArgs.Body.ToArray();
                var paymentInfoJson = Encoding.UTF8.GetString(byteArray);

                var paymentInfo = JsonSerializer.Deserialize<PaymentInfoInputModel>(paymentInfoJson);

                //processa pagamento
                ProcessPayment(paymentInfo);

                //Diz para o broker que a msg foi recebida
                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };
            //inicializa o consumo de MENSAGENS
            _channel.BasicConsume(QUEUE, false, consumer);
            
            return Task.CompletedTask;
        }

        public void ProcessPayment(PaymentInfoInputModel paymentInfo)
        {
            //cria um escopo para criar instancias que duram nesse escopo
            using (var scope = _serviceProvider.CreateScope())
            {
                //acessa o paymentService
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                
                //processa o pagamento
                paymentService.Process(paymentInfo);
            }
        }
    }
}
