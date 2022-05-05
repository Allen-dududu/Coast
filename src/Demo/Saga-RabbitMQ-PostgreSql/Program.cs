using Coast.RabbitMQ;
using Coast.PostgreSql;
using Saga_RabbitMQ_PostgreSql;
using Coast.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCosat(x =>
{
    x.DomainName = "Callback";
    x.UseRabbitMQ("localhost", "test",5);
    x.UsePostgreSql("Host=localhost;Port=5432;database=postgres;User Id=postgres;Password=root;"
);
});
builder.Services.AddTransient<TradeInEventHandle>();

var app = builder.Build();
app.CoastSubscribe<TradeInRequest,TradeInEventHandle>();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();


app.Run();
