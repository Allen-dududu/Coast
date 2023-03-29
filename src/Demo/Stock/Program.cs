
using Coast.Core;
using Coast.PostgreSql;
using Coast.RabbitMQ;
using Dapper;
using Npgsql;
using Stock.SagaEvents.EventHandling;
using Stock.SagaEvents.Events;

migrateDB();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCosat(x =>
{
    x.DomainName = "Stock";
    x.UseRabbitMQ("rabbitmq", "Stock", 5);
    x.UsePostgreSql("Host=db;Port=5432;User Id=postgres;database=stock;Password=postgres;"
);
});
builder.Services.AddTransient<ReduceStockEventHandler>();
builder.Services.AddTransient<ReduceStockCommitEventHandler>();


var app = builder.Build();
app.CoastSubscribe<ReduceStockEvent, ReduceStockEventHandler>();
app.CoastSubscribe<ReduceStockCommitEvent, ReduceStockCommitEventHandler>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

void migrateDB()
{
    string connectionString = "Host=db;Port=5432;User Id=postgres;Password=postgres;";
    string sql = @"Create Database ""stock""";

    using (var connection = new NpgsqlConnection(connectionString))
    {
        try
        {
            connection.Execute(sql);
        }
        catch (Exception ex)
        {
            // skip;
        }
    }
}
