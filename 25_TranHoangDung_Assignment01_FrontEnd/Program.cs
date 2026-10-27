using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using _25_TranHoangDung_Assignment01_FrontEnd.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Register Services
builder.Services.AddScoped<WordExportService>();

// Register HttpContextAccessor & Token Handler
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthTokenHandler>();

// Register HttpClient & ApiClient with JWT Bearer Token handler
builder.Services.AddHttpClient<NewsApiClient>()
    .AddHttpMessageHandler<AuthTokenHandler>();

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".FUNews.Session";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
