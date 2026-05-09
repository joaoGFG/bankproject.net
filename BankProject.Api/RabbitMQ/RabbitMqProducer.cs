using System.Text;
using RabbitMQ.Client;

namespace BankProject.Api.RabbitMQ;

public interface IRabbitMqProducer
{
    void PublishMessage(string queueName, string message);
    void PublicarFila(int contratacaoId);
}

public class RabbitMqProducer : IRabbitMqProducer
{
    public void PublishMessage(string queueName, string message)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);
    }

    public void PublicarFila(int contratacaoId)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "contratacao-solicitada",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var message = contratacaoId.ToString();
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                             routingKey: "contratacao-solicitada",
                             basicProperties: null,
                             body: body);
    }
}