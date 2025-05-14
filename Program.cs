using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using TicketManagementSystem.Middlewares;
using TicketManagementSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<EventContext>(opt =>
    opt.UseInMemoryDatabase("Event"));
builder.Services.AddDbContext<TicketContext>(opt =>
    opt.UseInMemoryDatabase("Ticket"));
builder.Services.AddDbContext<ReservationContext>(opt =>
    opt.UseInMemoryDatabase("Reservation"));

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});
app.UseHttpsRedirection();
// app.UseMiddleware<LoggingMiddleware>();
app.MapControllers();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
