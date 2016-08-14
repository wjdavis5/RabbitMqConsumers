using System;
using System.Collections.Concurrent;
using System.Threading;
using RabbitMqConsumers.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqConsumers.Consumers
{
    /// <summary>
    /// Extends <see cref="EventingBasicConsumer"/>. Adds messages to a <see cref="BlockingCollection{IRabbitMessage}"/> when received.
    /// </summary>
    public class QueueingEventConsumer : EventingBasicConsumer, IDisposable
    {
        public BlockingCollection<IRabbitMessage> Messages { get; set; }

        #region cTors

        /// <summary>
        /// Create a new <see cref="QueueingEventConsumer"/> 
        /// </summary>
        /// <param name="model">The IModel to use</param>
        public QueueingEventConsumer(IModel model) : base(model)
        {
            Messages = new BlockingCollection<IRabbitMessage>();
            base.Received += OnReceived;
        }

        /// <summary>
        /// Create a new <see cref="QueueingEventConsumer"/> 
        /// </summary>
        /// <param name="model">The IModel to use</param>
        /// <param name="messages">Provide your own <see cref="BlockingCollection{IRabbitMessage}"/></param>
        public QueueingEventConsumer(IModel model, BlockingCollection<IRabbitMessage> messages) : base(model)
        {
            Messages = messages;
        }
        #endregion

        #region Methods
        private void OnReceived(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            Enqueue(new RabbitMessage((IModel) sender, basicDeliverEventArgs));
        }

        public bool Enqueue(RabbitMessage rabbitMessage)
        {
            if (Messages.TryAdd(rabbitMessage)) return true;
            else return false;
        }

        
        public bool Enqueue(RabbitMessage rabbitMessage, TimeSpan timeout)
        {
            if (Messages.TryAdd(rabbitMessage,timeout)) return true;
            else return false;
        }
        public bool Enqueue(RabbitMessage rabbitMessage, int timeout)
        {
            if (Messages.TryAdd(rabbitMessage, timeout)) return true;
            else return false;
        }
        public bool Enqueue(RabbitMessage rabbitMessage, int timeout, CancellationToken cancellationToken)
        {
            if (Messages.TryAdd(rabbitMessage,timeout,cancellationToken)) return true;
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
            Messages.TryTake(out message,timeout);
            return message;
        }
        public IRabbitMessage Dequeue(int timeout)
        {
            IRabbitMessage message;
            Messages.TryTake(out message,timeout);
            return message;
        }
        public IRabbitMessage Dequeue(int timeout, CancellationToken cancellationToken)
        {
            IRabbitMessage message;
            Messages.TryTake(out message, timeout,cancellationToken);
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
