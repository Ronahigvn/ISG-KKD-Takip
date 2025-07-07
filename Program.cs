using ISGKkdTakip.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core - SQLite bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=isg.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // 🔧 app.MapStaticAssets() değil, bu kullanılmalı

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient();
