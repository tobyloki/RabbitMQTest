using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Producer;

public static class Program
{
    private static void Main(string[] args)
    {
        // Create a connection factory with the URI of your Amazon MQ RabbitMQ broker
        var factory = new ConnectionFactory()
        {
            Uri = new Uri("amqps://pdohatix:7I0Wco0RKRe8z4LC3uGx4k-7uqWqNCmL@beaver.rmq.cloudamqp.com/pdohatix")
        };

        // Create a connection and a channel
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var pinCode = "123456";
            
            // Declare the exchange and the queue
            var exchangeName = "msgExchange";
            var routingKey = $"key-{pinCode}";
            
            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
            
            // Add a callback for when the message is returned (e.g. if there is no queue bound to the exchange with the specified routing key)
            channel.BasicReturn += (sender, eventArgs) =>
            {
                var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var statusCode = eventArgs.ReplyCode;
                var reason = eventArgs.ReplyText;

                Console.WriteLine("Message returned: {0}", message);
                Console.WriteLine("Status code: {0}", statusCode);
                Console.WriteLine("Reason: {0}", reason);
            };

            // create infinite loop
            while (true)
            {
                // Create a message and convert it to a byte array
                // create random value
                var rnd = new Random();
                var value = rnd.Next(1, 100);
                // get timestamp local time with timezone
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffzzz");
                var message = new {Name = "John", Age = value, Timestamp = timestamp};
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                // Create a message properties object with the desired headers and properties
                var properties = channel.CreateBasicProperties();
                properties.ContentType = "text/plain";
                // It's worth noting that the durable attribute only applies to the queue or exchange itself, not to any messages that are sent or received. To ensure message durability, you should set the deliveryMode to 2 when publishing messages. This tells RabbitMQ to persist the message to disk so that it will not be lost in the event of a broker restart or failure.
                properties.DeliveryMode = 2;

                // Publish the message to the exchange with the desired routing key
                const bool mandatory = true;
                channel.BasicPublish(exchangeName, routingKey, mandatory, properties, body);

                Console.WriteLine("Sent message: {0}", message);
                
                // wait 5 seconds
                Thread.Sleep(5000);
            }
        }
    }
}