using Coast.RabbitMQ;
using Coast.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCosat(x =>
{
    x.UseRabbitMQ("localhost", "",5);
    x.UsePostgreSql("Host=localhost;Port=5432;database=postgres;User Id=postgres;Password=root;"
);
});

var app = builder.Build();



// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();


app.Run();
