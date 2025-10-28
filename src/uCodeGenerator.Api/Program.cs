using System.Text.Json;
using System.Text.Json.Serialization;
using Carter;
using uCodeGenerator.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Carter endpoints
builder.Services.AddCarter();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Our service from Core
builder.Services.AddScoped<UCodeGeneratorService>();

var app = builder.Build();

// Swagger UI only in Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global try/catch middleware from your code
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;
        var errorResponse = new
        {
            context.Response.StatusCode,
            ex.Message,
        };

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
});

app.UseCors();

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Carter routes
app.MapCarter();

app.UseHttpsRedirection();

app.Run();
