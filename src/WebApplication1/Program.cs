using Stock.SagaEvents.EventHandling;
using Stock.SagaEvents.Events;
using Coast.RabbitMQ;
using Coast.PostgreSql;
using Coast.Core;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCosat(x =>
{
    x.DomainName = "Stock";
    x.Schema = "public";
    x.UseRabbitMQ("localhost", "Stock", 5);
    x.UsePostgreSql("Host=localhost;Port=5432;database=Stock;User Id=postgres;Password=root;"
);
});
builder.Services.AddTransient<ReduceStockEventHandler>();

var app = builder.Build();
app.CoastSubscribe<ReduceStockRequest, ReduceStockEventHandler>();

app.MapGet("/", () => "Hello World!");

app.Run();
