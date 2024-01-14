
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ClimateTrackr_Server.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly RabbitMQConfig _configuration;
        public RabbitMQService(IOptions<RabbitMQConfig> options)
        {
            _configuration = options.Value;
        }

        IConnection IRabbitMQService.CreateChannel()
        {
            ConnectionFactory connection = new ConnectionFactory()
            {
                Uri = new Uri(_configuration.ConnectionUrl)
            };
            connection.DispatchConsumersAsync = true;
            var channel = connection.CreateConnection();
            return channel;
        }

        string IRabbitMQService.GetExName()
        {
            return _configuration.ExchangeName;
        }

        string IRabbitMQService.GetRoutingKey()
        {
            return _configuration.RoutingKey;
        }
    }
}