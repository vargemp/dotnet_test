using System.Text;
using MessageBus;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Reader;

class Program
{
    static async Task Main(string[] args)
    {
        var connFactory = new ConnectionFactory { HostName = "rabbitmq" };
        var msgService = new MessageService(connFactory);

        AsyncEventHandler<BasicDeliverEventArgs> msgHandler = (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Received {message}");
            return Task.CompletedTask;
        };

        await msgService.Listen(msgHandler);

        await Task.Delay(Timeout.Infinite);
    }
}
