using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MessageBus;

public class MessageService : IAsyncDisposable
{
    const string QUEUE_NAME = "DEFAULT_QUEUE";
    IChannel? channel;
    IConnection? connection;

    public MessageService(ConnectionFactory factory)
    {
        Console.WriteLine("MessageBus INITIALIZING4!");

        const int maxRetries = 10;
        int retries = 0;

        while (connection == null && retries < maxRetries)
        {
            try
            {
                connection = factory.CreateConnectionAsync().Result;
            }
            catch
            {
                retries++;
                Console.WriteLine($"RabbitMQ not ready yet, retrying in 2s... ({retries}/{maxRetries})");
                Task.Delay(2000).GetAwaiter().GetResult();
                Console.WriteLine("After 2 sec wait");
            }
        }

        if (connection == null)
            throw new Exception("Failed to connect to RabbitMQ after multiple attempts.");

        channel = connection.CreateChannelAsync().Result;

        channel.QueueDeclareAsync(queue: QUEUE_NAME, durable: false, exclusive: false, autoDelete: false,
        arguments: null).GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (channel is not null)
        {
            await channel.CloseAsync();
            channel.Dispose();
        }

        if (connection is not null)
        {
            await connection.CloseAsync();
            connection.Dispose();
        }
    }

    public async Task PublishMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        if (channel != null)
        {
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: QUEUE_NAME, body: body);
        }
    }

    public async Task Listen(AsyncEventHandler<BasicDeliverEventArgs> msgHandler)
    {
        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new AsyncEventingBasicConsumer(channel);
        // consumer.ReceivedAsync += msgHandler;
        consumer.ReceivedAsync += async (model, ea) =>
    {
        try
        {
            await msgHandler(model, ea);  // process message
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false); // ACK after success
        }
        catch (Exception ex)
        {
            Console.WriteLine($" [!] Error processing message: {ex}");
            // (optional) channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
        }
    };

        await channel.BasicConsumeAsync(QUEUE_NAME, autoAck: false, consumer: consumer);
    }
}
