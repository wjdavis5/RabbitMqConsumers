[![Build status](https://ci.appveyor.com/api/projects/status/v743qi8qdylq0nyu/branch/master?svg=true)](https://ci.appveyor.com/project/WilliamDavis/rabbitmqconsumers/branch/master)

# RabbitMqConsumers

Extra consumers for RabbitMQ. Have something to add? Submit a PR! Supports DOTNETCORE and NET451

Get the Nuget:
https://www.nuget.org/packages/RabbitMqConsumers


Example
---------


```csharp
public class Program
    {
        private static BlockingCollection<IRabbitMessage> MessageQueue { get; set; }
        private static IConnectionFactory RabbitConnectionFactory { get; set; }
        private static IConnection RabbitConnection { get; set; }
        private static Random _random = new Random();
        public static void Main(string[] args)
        {
            MessageQueue = new BlockingCollection<IRabbitMessage>();
            RabbitConnectionFactory = new ConnectionFactory() {Uri = "amqp://voioewrj:21-kYCYF3qsHYQR7svK5gtPodVlsvO0J@reindeer.rmq.cloudamqp.com/voioewrj" };
            RabbitConnection = RabbitConnectionFactory.CreateConnection();

            var rabbitChannel = RabbitConnection.CreateModel();
            rabbitChannel.QueueDeclareNoWait("ConsumerQueue", true, false, false, null);
            rabbitChannel.ExchangeDeclareNoWait("MessageExchange", "topic", true, false, null);
            rabbitChannel.QueueBindNoWait("ConsumerQueue", "MessageExchange", "*", null);

            var processThread = new Thread(ProcessMessages);
            processThread.Start();
            var producerThread = new Thread(ProduceMessages);
            producerThread.Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void ProcessMessages()
        {
            var rabbitChannel = RabbitConnection.CreateModel();
            var consumer = new QueueingConsumer(rabbitChannel, MessageQueue);
            rabbitChannel.BasicConsume("ConsumerQueue", false, consumer);
            while (true)
            {
                IRabbitMessage message;
                if (!MessageQueue.TryTake(out message, TimeSpan.FromSeconds(60))) continue;
                //do some work.....
                Console.WriteLine(Encoding.UTF8.GetString(message.BasicDeliverEventArgs.Body));
                message.RabbitChannel.BasicAck(message.BasicDeliverEventArgs.DeliveryTag, false);
            }
        }

        private static void ProduceMessages()
        {
            var rabbitChannel = RabbitConnection.CreateModel();
            var properties = rabbitChannel.CreateBasicProperties();
            rabbitChannel.ConfirmSelect();
            while (true)
            {
                rabbitChannel.BasicPublish("MessageExchange", "message", true, properties,
                    Encoding.UTF8.GetBytes(RandomString(10)));
                rabbitChannel.WaitForConfirms();
                Thread.Sleep(_random.Next(100,600));
            }
        }

        private static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

    }
```
