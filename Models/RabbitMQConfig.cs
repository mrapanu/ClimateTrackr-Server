namespace ClimateTrackr_Server.Models
{
    public class RabbitMQConfig
    {
        public string ConnectionUrl {get;set;}="amqp://guest:guest@localhost:5672/";
    }
}