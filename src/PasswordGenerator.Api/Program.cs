using PasswordGenerator.Api.Middleware;
using PasswordGenerator.Core.Interfaces;
using PasswordGenerator.Core.Models;
using PasswordGenerator.Infrastructure.Generators;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<PasswordGeneratorSettings>(
    builder.Configuration.GetSection("PasswordGenerator"));

// Register password generators
builder.Services.AddSingleton<IPasswordGenerator, PolicyPasswordGenerator>();
builder.Services.AddSingleton<IPasswordGenerator, UniformPasswordGenerator>();
builder.Services.AddSingleton<IPasswordGenerator, PassphrasePasswordGenerator>();

// Controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Strong Password Generator API",
        Version = "v1",
        Description = "REST API to generate strong passwords using three generation methods: policy, uniform, and passphrase."
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger UI (all environments for lab use)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Password Generator API v1");
    options.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
