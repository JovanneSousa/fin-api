using Configuration;
using Fin.Api.Configuration;
using Fin.Api.Configuration.Swagger;
using Fin.API.Configuration;
using Fin.API.Handlers;
using Fin.Application.Mapper;
using Jovanne.Jwks.Client;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services
    .AddApiConfig()
    .AddDiConfig()
    .AddSwaggerConfig()
    .AddDbContextConfig(configuration)
    .AddCorsConfig(configuration)
    .AddJovanneJwksClient(configuration, builder.Environment.IsDevelopment())
    .AddSerilogConfig(builder.Host)
    .AddAutoMapper(cfg =>
    {
        cfg.AddMaps(typeof(ApplicationAssemblyMarker).Assembly);
    });
builder.Services.
    AddProblemDetails()
    .AddExceptionHandler<GlobalExceptionHandler>();

await builder.Services
    .AddRabbitConfiguration(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler();

app.UseRouting();

app.UseCors("Production");

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
