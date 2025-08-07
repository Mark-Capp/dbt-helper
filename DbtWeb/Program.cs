using System.Text.Json.Serialization;
using Jinja2;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors();

var app = builder.Build();
app.UseCors(x =>
{
    x.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapGet("/render", () => Results.Ok());

app.MapPost("/render", ([FromBody] Request request, ILogger<Program> logger) =>
    {
        logger.LogInformation("Rendering");
        var template = Template.FromString(request.Content);
        var renderedContent = template.Render();
        
        logger.LogInformation("Rendered: {content}", renderedContent);
        return Results.Ok(new { Content =renderedContent});
    })
    .WithName("GetWeatherForecast");


app.Run();

public class Request
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}
