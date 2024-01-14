using RabbitMQ.Client;

namespace ClimateTrackr_Server.Services
{
    public interface IRabbitMQService
    {
         IConnection CreateChannel();
    }
}