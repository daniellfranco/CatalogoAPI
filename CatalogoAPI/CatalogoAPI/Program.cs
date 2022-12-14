using AutoMapper;
using CatalogoAPI.Context;
using CatalogoAPI.DTOs.Mappings;
using CatalogoAPI.Extensions;
using CatalogoAPI.Filters;
using CatalogoAPI.Logging;
using CatalogoAPI.Repository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Registrando o servi?o de filtro
builder.Services.AddScoped<ApiLoggingFilter>();

// Registrando o servi?o da Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Registrando o servi?o do AutoMapper
var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});
IMapper mapper = mappingConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// Registrando a Connection String
builder.Services.AddDbContext<CatalogoAPIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    // Adicional para resolver o problema da referencia ciclica
    // onde categoria referencia produto, e produto referencia categoria
    // e a serializa??o entra num loop de referencia.
    .AddJsonOptions(options =>
        options.JsonSerializerOptions
            .ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ativando o logging
// adiciona o provider e sua configura??o
builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration
{
    // atribui o nivel de log para information
    LogLevel = LogLevel.Information
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Chamando a extens?o do middleware
app.ConfigureExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
