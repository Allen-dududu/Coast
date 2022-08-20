
using Coast.Core;
using Coast.PostgreSql;
using Coast.RabbitMQ;
using Stock.SagaEvents.EventHandling;
using Stock.SagaEvents.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCosat(x =>
{
    x.DomainName = "Stock";
    x.UseRabbitMQ("localhost", "Stock", 5);
    x.UsePostgreSql("Host=localhost;Port=5432;database=Stock;User Id=coast;Password=coast;"
);
});
builder.Services.AddTransient<ReduceStockEventHandler>();

var app = builder.Build();
app.CoastSubscribe<ReduceStockEvent, ReduceStockEventHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
