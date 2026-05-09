using System.Text;
using System.Text.Json;
using BankProject.Api.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BankProject.Api.RabbitMQ;

public class ContratacaoConsumer : BackgroundService
{
    private readonly ILogger<ContratacaoConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection _connection;
    private IModel _channel;

    public ContratacaoConsumer(ILogger<ContratacaoConsumer> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        InitializeRabbitMqListener();
    }

    private void InitializeRabbitMqListener()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(queue: "contratacao-solicitada", durable: true, exclusive: false, autoDelete: false, arguments: null);
        
        _channel.BasicQos(0, 1, false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (ch, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Message received: {Content}", content);

            try
            {
                var payload = JsonSerializer.Deserialize<ContratacaoPayload>(content);
                
                if (payload != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var contratacao = await dbContext.Contratacoes.FindAsync(payload.ContratacaoId);
                    
                    if (contratacao != null)
                    {
                        int scoreSerasa = new Random().Next(1, 1000);
                        
                        _logger.LogInformation("Analyzing credit. Score: {Score}", scoreSerasa);

                        if (scoreSerasa >= 500)
                        {
                            contratacao.Status = "APROVADA";
                            _logger.LogInformation("Contratacao {Id} APPROVED.", contratacao.Id);
                        }
                        else
                        {
                            contratacao.Status = "RECUSADA - SCORE BAIXO";
                            _logger.LogInformation("Contratacao {Id} REFUSED by low score.", contratacao.Id);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume("contratacao-solicitada", autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

// Classe auxiliar de leitura da Fila
public class ContratacaoPayload
{
    public int ContratacaoId { get; set; }
    public int ClienteId { get; set; }
    public string Produto { get; set; }
}