using Configuration;
using Fin.Api.Configuration;
using Fin.Api.Configuration.Swagger;
using Fin.API.Handlers;
using Fin.Application.Mapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder
    .AddDiConfig()
    .AddSwaggerConfig()
    .AddDbContextConfig()
    .AddCorsConfig()
    .AddIdentityConfig()
    .AddSerilogConfig()
    .Services.AddAutoMapper(cfg =>
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
