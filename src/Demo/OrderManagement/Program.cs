
using OrderManagement.SagaEvents.EventHandling;
using OrderManagement.SagaEvents.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCosat(x =>
{
    x.DomainName = "OrderManagement";
    x.UseRabbitMQ("localhost");
    x.UsePostgreSql("Host=localhost;Port=5432;database=OrderManagement;User Id=coast;Password=coast;");
});

builder.Services.AddTransient<CreateOrderEventHandler>();

var app = builder.Build();
app.CoastSubscribe<CreateOrderEvent, CreateOrderEventHandler>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
