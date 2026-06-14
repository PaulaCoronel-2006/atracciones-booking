using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microservicios.Atracciones.Booking.DataAccess;       
using Microservicios.Atracciones.Booking.DataManagement;   
using Microservicios.Atracciones.Booking.Business;         
using Microservicios.Atracciones.Booking.API.Middleware;   
using Microsoft.OpenApi.Models;
using MassTransit;
using Serilog;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDataAccessServices(builder.Configuration);
builder.Services.AddDataManagementServices();
builder.Services.AddBusinessServices();

// Configurar MassTransit con RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitUri = builder.Configuration["RabbitMQ:Uri"];
        if (!string.IsNullOrEmpty(rabbitUri))
        {
            cfg.Host(new Uri(rabbitUri.Trim()));
        }
        else
        {
            var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
            var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
            var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

            cfg.Host(rabbitHost, "/", h =>
            {
                h.Username(rabbitUser);
                h.Password(rabbitPass);
            });
        }
    });
});

// Configurar OpenTelemetry Tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("Microservicios.Atracciones.Booking")
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("BookingService"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

builder.Services.AddControllers(options =>
{
    options.Filters.Add<Microservicios.Atracciones.Booking.API.Filters.ApiResponseWrapperFilter>();
    options.Conventions.Add(new RoutePrefixConvention());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Booking Microservice API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var jwtKey = (builder.Configuration["Jwt:Key"] ?? "BookingService_Super_Secret_Key_2026_Minimum_Length_Requirement_Long_String").Trim(); 
var jwtIssuer = (builder.Configuration["Jwt:Issuer"] ?? "BookingService").Trim();
var jwtAudience = (builder.Configuration["Jwt:Audience"] ?? "BookingServiceUsers").Trim();

Log.Information("JWT Configuration loaded - Issuer: {JwtIssuer}, Audience: {JwtAudience}", jwtIssuer, jwtAudience);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking Microservice API v1");
    c.RoutePrefix = string.Empty; 
});

app.UseMiddleware<ExceptionMiddleware>();
app.UseStaticFiles();

// UseHttpsRedirection omitido: servicio interno detrás de gateway nginx que maneja TLS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public class RoutePrefixConvention : Microsoft.AspNetCore.Mvc.ApplicationModels.IApplicationModelConvention
{
    public void Apply(Microsoft.AspNetCore.Mvc.ApplicationModels.ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    var currentTemplate = selector.AttributeRouteModel.Template;
                    if (currentTemplate != null && currentTemplate.StartsWith("api/v1/"))
                    {
                        selector.AttributeRouteModel.Template = currentTemplate["api/v1/".Length..];
                    }
                }
            }
        }
    }
}
