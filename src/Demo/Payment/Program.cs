using Coast.Core;
using Coast.PostgreSql;
using Coast.RabbitMQ;
using Payment.SagaEvents.EventHandling;
using Payment.SagaEvents.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCosat(x =>
{
    x.DomainName = "Payment";
    x.UseRabbitMQ("localhost", "Payment", 5);
    x.UsePostgreSql("Host=localhost;Port=5432;database=Payment;User Id=postgres;Password=root;"
);
});
builder.Services.AddTransient<DeductionEventHandler>();

var app = builder.Build();
app.CoastSubscribe<DeductionEvent, DeductionEventHandler>();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();


app.Run();
