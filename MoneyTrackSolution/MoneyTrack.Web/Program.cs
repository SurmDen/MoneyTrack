using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MoneyTrack.Application.Interfaces;
using MoneyTrack.Infrastructure.Data;
using MoneyTrack.Infrastructure.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddLogging();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MoneyTrack test API",
        Version = "v1",
        Description = "Test finance management API"
    });
});

builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLiteConnection"));
});

builder.Services.AddScoped<ICurrencyApiService, ExchangeRateApiService>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseRouting();
app.UseStaticFiles();
app.UseSession();
app.MapControllers();

app.MapGet("/main", () =>
{
    var filePath = Path.Combine(builder.Environment.WebRootPath, "mainpage", "index.html");

    return Results.File(filePath, "text/html");
});

app.Run();