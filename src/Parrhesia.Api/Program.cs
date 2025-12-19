using Parrhesia.Api.Extensions;
using Parrhesia.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApiServices(builder.Configuration);

// Add response compression
builder.Services.AddResponseCompression();

var app = builder.Build();

// Configure pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Parrhesia API v1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "Parrhesia API Documentation";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations in development
if (app.Environment.IsDevelopment())
{
    await app.Services.ApplyMigrationsAsync();
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Parrhesia API starting...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
