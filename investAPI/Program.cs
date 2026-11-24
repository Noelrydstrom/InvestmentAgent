using investAPI.Data;
using Microsoft.EntityFrameworkCore;
using investAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InvestmentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IInvestmentDataService, InvestmentDataService>();
builder.Services.AddScoped<ITradingService, TradingService>();

// Ollama AI
builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434");
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddCors(options =>
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();
app.UseCors("Angular");
app.UseAuthorization();
app.MapControllers();

app.Run();