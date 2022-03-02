using Coast.RabbitMQ;
using Coast.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCosat(x =>
{
    x.UseRabbitMQ("","",5);
    x.UsePostgreSql("");
});

var app = builder.Build();

app.MigrateCosat();


// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();


app.Run();
