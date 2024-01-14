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
            _model.QueueDeclare("climateTrackr", durable: true, exclusive: false, autoDelete: false);
            _model.ExchangeDeclare(rabbitMqService.GetExName(), ExchangeType.Direct, durable: true, autoDelete: false);
            _model.QueueBind("climateTrackr", rabbitMqService.GetExName(), rabbitMqService.GetRoutingKey());
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
                    await WriteToDatabaseAsync(message, dbContext);
                }
                await Task.CompletedTask;
                _model.BasicAck(ea.DeliveryTag, false);
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

        private async Task WriteToDatabaseAsync(TempAndHum message, DataContext context)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to database: {ex.Message}");
            }
        }
    }
}