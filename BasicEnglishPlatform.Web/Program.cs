using BasicEnglishPlatform.Data;
using BasicEnglishPlatform.Data.Repositories;
using BasicEnglishPlatform.Services.Implementations;
using BasicEnglishPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<AppDbContext>(options =>
//{
//    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//    //options.UseSqlServer(connectionString);
//    options.UseNpgsql(connectionString);
//});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Ki?m tra xem connection string c? ph?i l? d?ng URI (c?a Render) kh?ng
    // D?u hi?u: B?t ??u b?ng "postgres://"
    if (connectionString != null && connectionString.StartsWith("postgres://"))
    {
        try
        {
            var databaseUri = new Uri(connectionString);
            var userInfo = databaseUri.UserInfo.Split(':');

            // Chuy?n ??i sang ??nh d?ng chu?n c?a Npgsql
            // L?u ?: Render y?u c?u SSL, n?n c?n SslMode=Require v? Trust Server Certificate=true
            connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SslMode=Require;Trust Server Certificate=true;";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"L?i khi parse connection string c?a Render: {ex.Message}");
            // C? th? throw ho?c ?? nguy?n ?? n? b?o l?i g?c
        }
    }

    options.UseNpgsql(connectionString);
});


builder.Services.AddMemoryCache();

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddHttpClient<IAiService, GeminiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.ConnectionClose = true;
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,

    MaxConnectionsPerServer = 1,

    AutomaticDecompression = System.Net.DecompressionMethods.All
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "L?i khi ch?y Migration.");
    }
}

app.Run();
