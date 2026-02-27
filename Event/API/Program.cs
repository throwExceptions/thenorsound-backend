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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/event-service.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

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

EventEntityMapping.Configure();
DtoMappingConfiguration.Configure();

builder.Services.AddScoped<IValidator<CreateEventRequestDto>, CreateEventRequestDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateEventRequestDto>, UpdateEventRequestDtoValidator>();
builder.Services.AddScoped<IValidator<GetAllEventsRequestDto>, GetAllEventsRequestDtoValidator>();
builder.Services.AddScoped<IValidator<GetEventByIdRequestDto>, GetEventByIdRequestDtoValidator>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(typeof(CreateEventCommandHandler).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ThenorSound Event Service API",
        Version = "v1",
        Description = "Event management microservice for ThenorSound platform"
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Service API V1");
    c.RoutePrefix = string.Empty;
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
    Predicate = _ => false
});

app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = _ => true
});

app.Run();
