using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using AuctionServiceAPI.Models;
using AuctionServiceAPI.Services;
using AuctionServiceAPI.Interfaces;

public class BidReceiver : BackgroundService
{
    private readonly IModel _channel;
    private readonly ILogger<BidReceiver> _logger;
    private readonly IAuctionService _auctionService;

    public BidReceiver(ILogger<BidReceiver> logger, IAuctionService auctionService) 
    {
        _logger = logger;
        _auctionService = auctionService; 
        var factory = new ConnectionFactory { HostName = Environment.GetEnvironmentVariable("QueueHostName") }; 
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        _channel.QueueDeclare(queue: "bid_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($" Bid Received {message}");
            await HandleMessageAsync(message);
        };
        _channel.BasicConsume(queue: "bid_queue", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(string message)
    {
        _logger.LogInformation($"Bid received {message}");

        /* try
        {
            var bid = JsonSerializer.Deserialize<Bid>(message);

            if (bid != null)
            {
                await _auctionService.ProcessBidAsync(bid);
                _logger.LogInformation("Auction updated with new bid.");
            }
            else
            {
                _logger.LogError("Failed to deserialize bid.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing bid: {ex.Message}");
        } */
    }

    public override void Dispose()
    {
        _channel.Close();
        _channel.Dispose();
        base.Dispose();
    }
}