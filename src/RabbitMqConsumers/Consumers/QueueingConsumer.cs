using System;
using System.Collections.Concurrent;
using System.Threading;
using RabbitMqConsumers.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqConsumers.Consumers
{
    /// <summary>
    /// Extends <see cref="DefaultBasicConsumer"/>. Adds messages to a <see cref="BlockingCollection{IRabbitMessage}"/> when received.
    /// </summary>
    public class QueueingConsumer : DefaultBasicConsumer, IDisposable
    {

        #region
        ///<summary>Event fired on HandleBasicConsumeOk.</summary>
        public event EventHandler<ConsumerEventArgs> Registered;

        ///<summary>Event fired on HandleModelShutdown.</summary>
        public event EventHandler<ShutdownEventArgs> Shutdown;

        ///<summary>Event fired on HandleBasicCancelOk.</summary>
        public event EventHandler<ConsumerEventArgs> Unregistered;
        #endregion

        public BlockingCollection<IRabbitMessage> Messages { get; set; }

        #region cTors
        /// <summary>
        /// Create a new <see cref="QueueingConsumer"/> 
        /// </summary>
        /// <param name="model">The IModel to use</param>
        public QueueingConsumer(IModel model) : base(model)
        {
            Messages = new BlockingCollection<IRabbitMessage>();
        }

        /// <summary>
        /// Create a new <see cref="QueueingConsumer"/> 
        /// </summary>
        /// <param name="model">The IModel to use</param>
        /// <param name="messages">Provide your own <see cref="BlockingCollection{IRabbitMessage}"/></param>
        public QueueingConsumer(IModel model, BlockingCollection<IRabbitMessage> messages) : base(model)
        {
            Messages = messages;
        }
        #endregion

        #region Methods
        ///<summary>Fires the Unregistered event.</summary>
        public override void HandleBasicCancelOk(string consumerTag)
        {
            base.HandleBasicCancelOk(consumerTag);

            Unregistered?.Invoke(this, new ConsumerEventArgs(consumerTag));
        }
        ///<summary>Fires the Registered event.</summary>
        public override void HandleBasicConsumeOk(string consumerTag)
        {
            base.HandleBasicConsumeOk(consumerTag);

            Registered?.Invoke(this, new ConsumerEventArgs(consumerTag));
        }
        ///<summary>Fires the Shutdown event.</summary>
        public override void HandleModelShutdown(object model, ShutdownEventArgs reason)
        {
            base.HandleModelShutdown(model, reason);

            Shutdown?.Invoke(this, reason);
        }
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
           finally
            {
                if (!isQueued)
                {
                    Model.BasicNack(deliveryTag, false, true);
                }
            }

        }

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
        public IRabbitMessage Dequeue()
        {
            IRabbitMessage message;
            Messages.TryTake(out message);
            return message;
        }
        public IRabbitMessage Dequeue(TimeSpan timeout)
        {
            IRabbitMessage message;
            Messages.TryTake(out message, timeout);
            return message;
        }
        public IRabbitMessage Dequeue(int timeout)
        {
            IRabbitMessage message;
            Messages.TryTake(out message, timeout);
            return message;
        }
        public IRabbitMessage Dequeue(int timeout, CancellationToken cancellationToken)
        {
            IRabbitMessage message;
            Messages.TryTake(out message, timeout, cancellationToken);
            return message;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Messages.Dispose();
            }
        }
        #endregion
    }


}