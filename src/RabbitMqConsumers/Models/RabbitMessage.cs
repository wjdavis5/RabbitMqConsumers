using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqConsumers.Models
{
    public struct RabbitMessage : IRabbitMessage
    {
        public IModel RabbitChannel { get; set; }
        public BasicDeliverEventArgs BasicDeliverEventArgs { get; set; }

        public RabbitMessage(IModel rabbitChannel, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            RabbitChannel = rabbitChannel;
            BasicDeliverEventArgs = basicDeliverEventArgs;
        }
    }
}