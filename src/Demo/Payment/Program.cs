using Coast.Core;
using Coast.PostgreSql;
using Coast.RabbitMQ;
using Dapper;
using Npgsql;
using Payment.SagaEvents.EventHandling;
using Payment.SagaEvents.Events;

migrateDB();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCosat(x =>
{
    x.DomainName = "Payment";
    x.UseRabbitMQ("rabbitmq", "Payment", 5);
    x.UsePostgreSql("Host=db;Port=5432;User Id=postgres;database=payment;Password=postgres;"
);
});
builder.Services.AddTransient<DeductionEventHandler>();

var app = builder.Build(); 
app.CoastSubscribe<DeductionEvent, DeductionEventHandler>();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

void migrateDB()
{
    string connectionString = "Host=db;Port=5432;User Id=postgres;Password=postgres;";
    string sql = @"Create Database ""payment""";

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