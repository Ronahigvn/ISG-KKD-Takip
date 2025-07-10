using ISGKkdTakip.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC ve API controllerları için
builder.Services.AddControllersWithViews();

// DbContext için bağlantı dizesi
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=isg.db"));

// IHttpClientFactory kullanımı için HttpClient servisini ekle
builder.Services.AddHttpClient();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// API controllerların attribute routing için bu satır şart
app.MapControllers();

// Klasik MVC controller rotası
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
