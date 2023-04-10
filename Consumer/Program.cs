using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Consumer;

public static class Program
{
    private static void Main(string[] args)
    {
        var factory = new ConnectionFactory()
        {
            Uri = new Uri("amqps://pdohatix:7I0Wco0RKRe8z4LC3uGx4k-7uqWqNCmL@beaver.rmq.cloudamqp.com/pdohatix")
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        var pinCode = "123456";

        var exchangeName = "msgExchange";
        var queueName = $"queue-{pinCode}";
        var routingKey = $"key-{pinCode}";
        
        channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
        channel.QueueDeclare(queueName, durable: true, exclusive: true, autoDelete: true, arguments: null);
        channel.QueueBind(queueName, exchangeName, routingKey);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(body));
                Console.WriteLine("Received message: {0}", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling message: {0}", ex.Message);
                // You may want to implement additional error handling logic here,
                // such as logging the error or sending a message to an error queue.
            }
        };
        channel.BasicConsume(queue: queueName,
            autoAck: true,
            consumer: consumer);

        Console.WriteLine("Consumer started. Press any key to stop...");
        Console.ReadKey();

        // Clean up resources
        channel.Close();
        connection.Close();
    }
}