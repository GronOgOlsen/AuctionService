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

        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("QueueHostName") ?? "localhost", // Tilføj standardværdi
            UserName = "guest",
            Password = "guest"
        };

        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        _channel.QueueDeclare(queue: "BiddingQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Bid Received: {message}");

            try
            {
                await HandleMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling bid message: {ex.Message}");
            }
        };

        _channel.BasicConsume(queue: "BiddingQueue", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(string message)
    {
        try
        {
            // Deserialize the message into a Bid object
            var bid = JsonSerializer.Deserialize<Bid>(message);

            if (bid == null)
            {
                _logger.LogWarning("Received bid message was null or invalid.");
                return;
            }

            // Process the bid with the auction service
            _logger.LogInformation($"Processing bid for AuctionId: {bid.AuctionId} with amount: {bid.Amount}");

            var result = await _auctionService.ProcessBidAsync(bid);

            if (result)
            {
                _logger.LogInformation("Auction updated successfully with new bid.");
            }
            else
            {
                _logger.LogWarning("Auction update failed. Either auction not found or bid was invalid.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to process bid: {ex.Message}");
        }
    }

    public override void Dispose()
    {
        _channel.Close();
        _channel.Dispose();
        base.Dispose();
    }
}
