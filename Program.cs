using fin_api.Configuration;
using fin_api.Configuration.Mapper;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder
    .AddDiConfig()
    .AddSwaggerConfig()
    .AddDbContextConfig()
    .AddCorsConfig()
    .AddIdentityConfig()
    .Services.AddAutoMapper(cfg =>
    {
        cfg.AddMaps(typeof(Program).Assembly);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseCors("Development");
    app.UseSwagger();
    app.UseSwaggerUI();

}
app.UseCors("Production");

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

app.Run();
