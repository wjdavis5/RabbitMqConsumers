using System;
using System.Collections.Concurrent;
using System.Threading;
using RabbitMqConsumers.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqConsumers
{
    public class QueueingConsumer : DefaultBasicConsumer
    {
        public BlockingCollection<IRabbitMessage> Messages { get; set; }

        #region cTors
        public QueueingConsumer(IModel model) : base(model)
        {
            Messages = new BlockingCollection<IRabbitMessage>();
        }

        public QueueingConsumer(IModel model, BlockingCollection<IRabbitMessage> messages) : base(model)
        {
            Messages = messages;
        }
        #endregion

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
            IBasicProperties properties, byte[] body)
        {
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            var basicDeliverEventArgs = new BasicDeliverEventArgs
            {
                BasicProperties = properties,
                Body = body,
                ConsumerTag = consumerTag,
                DeliveryTag = deliveryTag,
                Exchange = exchange,
                Redelivered = redelivered,
                RoutingKey = routingKey
            };
            var message = new RabbitMessage(Model,basicDeliverEventArgs);
            var isQueued = false;
            try
            {
                isQueued = Enqueue(message);
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                if (!isQueued)
                {
                    Model.BasicNack(deliveryTag, false, true);
                }
            }

        }

        #region Methods
        public bool Enqueue(RabbitMessage rabbitMessage)
        {
            if (Messages.TryAdd(rabbitMessage)) return true;
            else return false;
        }


        public bool Enqueue(RabbitMessage rabbitMessage, TimeSpan timeout)
        {
            if (Messages.TryAdd(rabbitMessage, timeout)) return true;
            else return false;
        }
        public bool Enqueue(RabbitMessage rabbitMessage, int timeout)
        {
            if (Messages.TryAdd(rabbitMessage, timeout)) return true;
            else return false;
        }
        public bool Enqueue(RabbitMessage rabbitMessage, int timeout, CancellationToken cancellationToken)
        {
            if (Messages.TryAdd(rabbitMessage, timeout, cancellationToken)) return true;
            else return false;
        }
        #endregion
    }


}