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

    // Konstruktoren initialiserer RabbitMQ-kanalen og tilknytter den til køen "BiddingQueue".
    public BidReceiver(ILogger<BidReceiver> logger, IAuctionService auctionService)
    {
        _logger = logger;
        _auctionService = auctionService;

        // Opretter en RabbitMQ-forbindelse ved hjælp af konfigurationsvariabler
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("QueueHostName") ?? "localhost", // Standardværdi hvis miljøvariabel mangler
            UserName = "guest", // RabbitMQ-standardbrugernavn
            Password = "guest"  // RabbitMQ-standardadgangskode
        };

        // Initialiserer RabbitMQ-kanalen
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();

        // Deklarerer en kø ved navn "BiddingQueue", hvis den ikke allerede eksisterer
        _channel.QueueDeclare(queue: "BiddingQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    // Starter baggrundsprocessen, der lytter på RabbitMQ-køen
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        // Definerer hvad der skal ske, når en besked modtages
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray(); // Læser beskedens body
            var message = Encoding.UTF8.GetString(body); // Konverterer til string
            _logger.LogInformation($"Bid Received: {message}");

            try
            {
                // Håndterer beskeden
                await HandleMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling bid message: {ex.Message}");
            }
        };

        // Starter forbrugeren til at lytte på "BiddingQueue"
        _channel.BasicConsume(queue: "BiddingQueue", autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }

    // Håndterer en enkelt besked ved at deserialisere og behandle den
    private async Task HandleMessageAsync(string message)
    {
        try
        {
            // Deserialiserer beskeden til et Bid-objekt
            var bid = JsonSerializer.Deserialize<Bid>(message);

            if (bid == null)
            {
                _logger.LogWarning("Received bid message was null or invalid.");
                return;
            }

            // Behandler buddet ved hjælp af AuctionService
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

    // Rydder ressourcer, når tjenesten stoppes
    public override void Dispose()
    {
        _channel.Close(); // Lukker RabbitMQ-kanalen
        _channel.Dispose(); // Frigør ressourcer
        base.Dispose(); // Kald til base-klassens Dispose-metode
    }
}
