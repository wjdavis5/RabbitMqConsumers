using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqConsumers.Models
{
    public interface IRabbitMessage
    {
        IModel RabbitChannel { get; set; }
        BasicDeliverEventArgs BasicDeliverEventArgs { get; set; }
    }
}