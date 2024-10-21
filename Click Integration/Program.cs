using Click_Integration;
using Click_Integration.Middlewares;
using Click_Integration.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TelegramBotClient>(provider =>
{
    var botToken = $"8082103495:AAHbyuRCCp0UQ1CGTDpSHVi2RuYF87EVaj4";
    return new TelegramBotClient(botToken);
});

builder.Services.AddSingleton<ITelegramService, TelegramService>();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

//builder.Logging.ClearProviders();

builder.Logging.AddSerilog(logger);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(x =>
    x.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
