using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.MappingConfiguration;
using Application.Commands;
using Domain.MappingConfiguration;
using FluentValidation;
using Infra;
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
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Service API V1");
    c.RoutePrefix = string.Empty; // Swagger UI at root
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();