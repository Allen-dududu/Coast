using Coast.RabbitMQ;
using Coast.PostgreSql;
using Coast.Core;
using Payment.SagaEvents.EventHandling;
using Payment.SagaEvents.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCosat(x =>
{
    x.DomainName = "Payment";
    x.Schema = "public";

    x.UseRabbitMQ("localhost", "Payment", 5);
    x.UsePostgreSql("Host=localhost;Port=5432;database=Payment;User Id=postgres;Password=root;"
);
});
builder.Services.AddTransient<DeductionEventHandler>();

var app = builder.Build();
app.CoastSubscribe<DeductionRequest, DeductionEventHandler>();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();


app.Run();
