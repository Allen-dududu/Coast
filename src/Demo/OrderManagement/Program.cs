
using Dapper;
using Npgsql;
using OrderManagement.SagaEvents.EventHandling;
using OrderManagement.SagaEvents.Events;

migrateDB();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCosat(x =>
{
    x.DomainName = "OrderManagement";
    x.UseRabbitMQ("rabbitmq");
    x.UsePostgreSql("Host=db;Port=5432;User Id=postgres;database=order;Password=postgres;");
});

builder.Services.AddTransient<CreateOrderEventHandler>();

var app = builder.Build();
app.CoastSubscribe<CreateOrderEvent, CreateOrderEventHandler>();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();

void migrateDB()
{
    string connectionString = "Host=db;Port=5432;User Id=postgres;Password=postgres;";
    string sql = @"Create Database ""order""";

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