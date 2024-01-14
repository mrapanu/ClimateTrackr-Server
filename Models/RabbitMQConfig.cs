namespace ClimateTrackr_Server.Models
{
    public class RabbitMQConfig
    {
        public string ConnectionUrl { get; set; } = "amqp://guest:guest@localhost:5672/";
        public string ExchangeName { get; set; } = "climateTrackr_ex";
        public string RoutingKey { get; set; } = "climateTrackr_ex";
    }
}