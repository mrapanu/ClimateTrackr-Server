using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClimateTrackr_Server.Services
{
    public class ConsumerService : IConsumerService, IDisposable
    {
        private readonly IModel _model;
        private readonly IConnection _connection;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public ConsumerService(IRabbitMQService rabbitMqService, IServiceScopeFactory serviceScopeFactory)
        {
            _connection = rabbitMqService.CreateChannel();
            _model = _connection.CreateModel();
            _model.QueueDeclare("climateTrackr", durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", "climateTrackr_dlx"},
                {"x-dead-letter-routing-key", rabbitMqService.GetRoutingKey()},
                {"x-message-ttl", 300000}
            });
            _model.QueueDeclare("climateTrackrDLQ", durable: true, exclusive: false, autoDelete: false);
            _model.ExchangeDeclare("climateTrackr_dlx", ExchangeType.Direct, durable: true, autoDelete: false);
            _model.ExchangeDeclare(rabbitMqService.GetExName(), ExchangeType.Direct, durable: true, autoDelete: false);
            _model.QueueBind("climateTrackr", rabbitMqService.GetExName(), rabbitMqService.GetRoutingKey());
            _model.QueueBind("climateTrackrDLQ", "climateTrackr_dlx", rabbitMqService.GetRoutingKey());
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task ReadMessgaes()
        {
            var consumer = new AsyncEventingBasicConsumer(_model);
            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject<TempAndHum>(Encoding.UTF8.GetString(body));

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                    await WriteToDatabaseAsync(message!, dbContext, ea.DeliveryTag);
                }
                await Task.CompletedTask;

            };
            _model.BasicConsume("climateTrackr", false, consumer);
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_model.IsOpen)
                _model.Close();
            if (_connection.IsOpen)
                _connection.Close();
        }

        private async Task WriteToDatabaseAsync(TempAndHum message, DataContext context, ulong deliveryTag)
        {
            try
            {
                // Create a new instance of your entity class and populate its properties
                var tempAndHum = new TempAndHum
                {
                    Room = message.Room,
                    Date = message.Date,
                    Temperature = message.Temperature,
                    Humidity = message.Humidity
                };

                // Add the entity to the DbContext and save changes asynchronously
                context.TempAndHums.Add(tempAndHum);
                await context.SaveChangesAsync();
                await Task.CompletedTask;
                _model.BasicAck(deliveryTag, false);
            }
            catch (Exception ex)
            {
                _model.BasicReject(deliveryTag, false);
                Console.WriteLine($"Error writing to database: {ex.Message}");
            }
        }
    }
}