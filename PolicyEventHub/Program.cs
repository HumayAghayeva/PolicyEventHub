
using MassTransit;
using RabbitMQ.Client;
using PolicyEventHub.Extensions;
using PolicyEventHub.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace PolicyEventHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMassTransit(cfg =>
            {
                cfg.UsingRabbitMq((context, bus) =>
                {
                    var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                    bus.Host(options.Host, options.Port, options.VirtualHost, h =>
                    {
                        h.Username(options.Username);
                        h.Password(options.Password);

#if !DEBUG
                  h.UseSsl(s =>
                  {
                      s.Protocol = SslProtocols.Tls12;
                      s.AllowPolicyErrors(SslPolicyErrors.None);
                  });
#endif
                    });

                    //bus.Message<ApprovalDecidedEvent>(x =>
                    //{
                    //    x.SetEntityName(options.Exchange);
                    //});

                    //bus.Publish<ApprovalDecidedEvent>(x =>
                    //{
                    //    x.ExchangeType = ExchangeType.Topic;
                    //});

                });
            });
            var app = builder.Build();

            app.UseCorrelationId();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseGlobalErrorHandling();

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.UseRouting();


            app.MapControllers();

            app.Run();
        }
    }
}
