﻿using MassTransit;
using System.Text;
using UserService.Api.Context;

///using RabbitMQ.Client;

namespace UserService.Api.Services
{
    public class IntegrationEventSenderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private CancellationTokenSource _wakeupCancelationTokenSource = new();

        public IntegrationEventSenderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            using var scope = _scopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceContext>();
            dbContext.Database.EnsureCreated();
        }

        public void StartPublishingOutstandingIntegrationEvents()
        {
            _wakeupCancelationTokenSource.Cancel();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await PublishOutstandingIntegrationEvents(stoppingToken);
            }
        }

        private async Task PublishOutstandingIntegrationEvents(CancellationToken stoppingToken)
        {
            try
            {
                ///var factory = new ConnectionFactory();
                ///var connection = factory.CreateConnection();
                ///var channel = connection.CreateModel();
                ///channel.ConfirmSelect(); // enable publisher confirms
                ///IBasicProperties props = channel.CreateBasicProperties();
                ///props.Persistent = true;
                ///props.DeliveryMode = 2; // persist message

                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    using var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceContext>();
                    var events = dbContext.IntegrationEvents.OrderBy(o => o.Id).ToList();
                    foreach (var e in events)
                    {
                        var body = Encoding.UTF8.GetBytes(e.Data);
                        ///channel.BasicPublish(exchange: "user",
                        ///    routingKey: e.Event,
                        ///    basicProperties: props,
                        ///    body: body);
                        ///channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5)); // wait 5 seconds for publisher confirm
                        await _publishEndpoint.Publish<byte[]>(body);
                        Console.WriteLine("Published: " + e.Event + " " + e.Data);
                        dbContext.Remove(e);
                        dbContext.SaveChanges();
                    }

                    ///await Task.Delay(5000, stoppingToken);
                    using var linkedCts = CancellationTokenSource
                        .CreateLinkedTokenSource(_wakeupCancelationTokenSource.Token, stoppingToken);
                    try
                    {
                        await Task.Delay(Timeout.Infinite, linkedCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        if (_wakeupCancelationTokenSource.Token.IsCancellationRequested)
                        {
                            Console.WriteLine("Publish requested");
                            var tmp = _wakeupCancelationTokenSource;
                            _wakeupCancelationTokenSource = new CancellationTokenSource();
                            tmp.Dispose();
                        }
                        else if (stoppingToken.IsCancellationRequested)
                        {
                            Console.WriteLine("Shutting down.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}