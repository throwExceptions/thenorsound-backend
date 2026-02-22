using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.MappingConfiguration;
using Application.Commands;
using Domain.MappingConfiguration;
using FluentValidation;
using Infra;
using Infra.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/customer-service.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Mapster mappings
CustomerEntityMapping.Configure();
DtoMappingConfiguration.Configure();

// Register FluentValidation validators
builder.Services.AddScoped<IValidator<CreateCustomerRequestDto>, CreateCustomerRequestDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateCustomerRequestDto>, UpdateCustomerRequestDtoValidator>();
builder.Services.AddScoped<IValidator<GetAllCustomersRequestDto>, GetAllCustomersRequestDtoValidator>();
builder.Services.AddScoped<IValidator<GetCustomerByIdRequestDto>, GetCustomerByIdRequestDtoValidator>();

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(typeof(CreateCustomerCommandHandler).Assembly));

// Register Infrastructure services (MongoDB, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb");

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ThenorSound Customer Service API",
        Version = "v1",
        Description = "Customer management microservice for ThenorSound platform"
    });
});

var app = builder.Build();

// Configure HTTP request pipeline
app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Service API V1");
    c.RoutePrefix = string.Empty; // Swagger UI at root
});

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/healthz/live", new HealthCheckOptions
{
    Predicate = _ => false // Liveness: always OK if process is running
});

app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = _ => true // Readiness: checks all registered health checks
});

app.Run();
