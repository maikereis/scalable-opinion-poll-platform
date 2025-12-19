# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY Parrhesia.Api.slnx ./
COPY src/Parrhesia.Api/Parrhesia.Api.csproj src/Parrhesia.Api/
COPY src/Parrhesia.Application/Parrhesia.Application.csproj src/Parrhesia.Application/
COPY src/Parrhesia.Domain/Parrhesia.Domain.csproj src/Parrhesia.Domain/
COPY src/Parrhesia.Infrastructure/Parrhesia.Infrastructure.csproj src/Parrhesia.Infrastructure/

# Restore dependencies
RUN dotnet restore src/Parrhesia.Api/Parrhesia.Api.csproj

# Copy everything else
COPY . .

# Build
WORKDIR /src/src/Parrhesia.Api
RUN dotnet build -c Release -o /app/build --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --no-build /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r parrhesia && useradd -r -g parrhesia parrhesia

# Copy published app
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R parrhesia:parrhesia /app

# Switch to non-root user
USER parrhesia

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Parrhesia.Api.dll"]
