# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY PasswordGenerator.sln .
COPY src/PasswordGenerator.Api/PasswordGenerator.Api.csproj src/PasswordGenerator.Api/
COPY src/PasswordGenerator.Core/PasswordGenerator.Core.csproj src/PasswordGenerator.Core/
COPY src/PasswordGenerator.Infrastructure/PasswordGenerator.Infrastructure.csproj src/PasswordGenerator.Infrastructure/
COPY tests/PasswordGenerator.Tests/PasswordGenerator.Tests.csproj tests/PasswordGenerator.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all remaining source code
COPY . .

# Build and publish release version
RUN dotnet publish src/PasswordGenerator.Api/PasswordGenerator.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Copy wordlist file needed by PassphraseGenerator
COPY --from=build /src/src/PasswordGenerator.Infrastructure/WordList/eff-large-wordlist.txt ./WordList/

# Expose port 8081
EXPOSE 8081

# Set environment to production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8081

# Run the application
ENTRYPOINT ["dotnet", "PasswordGenerator.Api.dll"]
