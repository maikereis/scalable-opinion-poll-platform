using Microsoft.EntityFrameworkCore;
using Parrhesia.Application.Common;
using Parrhesia.Application.SurveyManagement.ActivateSurvey;
using Parrhesia.Application.SurveyManagement.AddOption;
using Parrhesia.Application.SurveyManagement.AddQuestion;
using Parrhesia.Application.SurveyManagement.CreateSurvey;
using Parrhesia.Application.SurveyManagement.GetSurvey;
using Parrhesia.Domain.Common;
using Parrhesia.Infrastructure;
using Parrhesia.Infrastructure.Persistence;

namespace Parrhesia.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();
        
        // API Explorer & Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Parrhesia API",
                Version = "v1",
                Description = "Survey and voting platform API",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Parrhesia Team",
                    Email = "support@parrhesia.com"
                }
            });

            // Add XML comments if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        // CORS Configuration
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(
                        "X-RateLimit-Limit",
                        "X-RateLimit-Remaining",
                        "X-RateLimit-Reset",
                        "Retry-After");
            });
        });

        // Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<ParrhesiaDbContext>();

        // Infrastructure Layer
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
        
        var fingerprintSalt = configuration["Security:FingerprintSalt"]
            ?? throw new InvalidOperationException("FingerprintSalt configuration not found");

        services.AddInfrastructure(connectionString, fingerprintSalt);

        // Application Use Cases
        services.AddScoped<IUseCase<CreateSurveyRequest, Result<CreateSurveyResponse>>, CreateSurveyUseCase>();
        services.AddScoped<IUseCase<GetSurveyRequest, Result<GetSurveyResponse>>, GetSurveyUseCase>();
        services.AddScoped<IUseCase<AddQuestionRequest, Result<AddQuestionResponse>>, AddQuestionUseCase>();
        services.AddScoped<IUseCase<AddOptionRequest, Result<AddOptionResponse>>, AddOptionUseCase>();
        services.AddScoped<IUseCase<ActivateSurveyRequest, Result<ActivateSurveyResponse>>, ActivateSurveyUseCase>();

        // Response Compression
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ParrhesiaDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        int maxRetries = 10;
        int delaySeconds = 5;

        for (int i = 1; i <= maxRetries; i++)
        {
            try
            {
                logger.LogInformation("Tentativa {Attempt} de aplicar migrações...", i);
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Banco de dados sincronizado com sucesso.");
                return; // Sucesso, sai do método
            }
            catch (Exception ex)
            {
                logger.LogWarning("O banco 'Parrhesia' ainda não está pronto ou disponível. Erro: {Message}", ex.Message);
                
                if (i == maxRetries)
                {
                    logger.LogCritical("Não foi possível conectar ao banco após {Max} tentativas.", maxRetries);
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}