using Microsoft.EntityFrameworkCore;
using PostService.Context;
using PostService.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json.Nodes;

namespace PostService
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            ///builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            ///builder.Services.AddEndpointsApiExplorer();
            ///builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<PostServiceContext>(options =>
                options.UseSqlServer(@"Data Source=(local);Initial Catalog=Posts;User Id=sa;Password=tbm123;Encrypt=false"));
            ///builder.Services.AddSingleton<IntegrationEventSenderService>();
            ///builder.Services.AddHostedService<IntegrationEventSenderService>(provider => provider.GetService<IntegrationEventSenderService>());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            ///if (app.Environment.IsDevelopment())
            ///{
            ///    app.UseSwagger();
            ///    app.UseSwaggerUI();
            ///}

            ///app.UseHttpsRedirection();
            ///app.UseAuthorization();
            ///app.MapControllers();

            ListenForIntegrationEvents();
            app.Run();
            ///CreateHostBuilder(args).Build().Run();
        }

        private static void ListenForIntegrationEvents()
        {
            var factory = new ConnectionFactory();
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var contextOptions = new DbContextOptionsBuilder<PostServiceContext>()
                   .UseSqlServer(@"Data Source=(local);Initial Catalog=Posts;User Id=sa;Password=tbm123;Encrypt=false;")
                   .Options;
                var dbContext = new PostServiceContext(contextOptions);
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
                var data = JsonNode.Parse(message)?.AsObject(); ///JObject.Parse(message);
                                                                ///Console.WriteLine(data[0]);
                if (data != null)
                {
                    var type = ea.RoutingKey;
                    if (type == "user.add")
                    {
                        if (dbContext.Users.Any(a => a.Id == data["Id"].GetValue<int>()))
                        {
                            Console.WriteLine("Ignoring old/duplicate entity");
                        }
                        else
                        {
                            dbContext.Users.Add(new User()
                            {
                                Id = data["Id"].GetValue<int>(),
                                Name = data["Name"].GetValue<string>(),
                                Email = data["Email"].GetValue<string>(),
                                Version = 1///data["Version"].Value<int>()
                            });
                            dbContext.SaveChanges();
                        }
                    }
                    else if (type == "user.update")
                    {
                        ///int newVersion = data["version"].Value<int>();
                        var user = dbContext.Users.First(a => a.Id == data["Id"].GetValue<int>());
                        ///if (user.Version >= newVersion)
                        ///{
                        ///    Console.WriteLine("Ignoring old/duplicate entity");
                        ///}
                        ///else
                        ///{
                        user.Name = data["Name"].GetValue<string>();
                        user.Email = data["Email"].GetValue<string>();
                        user.Version = user.Version + 1;///newVersion;
                        dbContext.SaveChanges();
                        ///}
                    }
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            };
            channel.BasicConsume(queue: "user",
                autoAck: false,
                consumer: consumer);
        }

        ////public static IHostBuilder CreateHostBuilder(string[] args) =>
        ////    Host.CreateDefaultBuilder(args)
        ////        .ConfigureWebHostDefaults(webBuilder =>
        ////        {
        ////            webBuilder.UseStartup<Startup>();
        ////        });
    }
}