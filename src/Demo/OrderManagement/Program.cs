using Coast.RabbitMQ;
using Coast.PostgreSql;
using Coast.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCosat(x =>
{
    x.DomainName = "OrderManagement";
    x.Schema = "public";

    x.UseRabbitMQ("localhost", "OrderManagement", 5);
    x.UsePostgreSql("Host=localhost;Port=5432;database=OrderManagement;User Id=postgres;Password=root;"
);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();