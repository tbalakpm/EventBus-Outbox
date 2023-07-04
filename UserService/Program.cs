using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Context;
using UserService.Api.Services;

namespace UserService.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq();
            });
            ///builder.Services.AddMassTransitHostedService();
            builder.Services.AddDbContext<UserServiceContext>(options =>
                options.UseSqlServer(@"Data Source=(local);Initial Catalog=Sales;User Id=sa;Password=tbm123;Encrypt=false"));
            builder.Services.AddSingleton<IntegrationEventSenderService>();
            builder.Services.AddHostedService(provider => provider.GetService<IntegrationEventSenderService>()!);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}