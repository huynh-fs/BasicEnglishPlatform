using BasicEnglishPlatform.Data;
using BasicEnglishPlatform.Data.Repositories;
using BasicEnglishPlatform.Services.Implementations;
using BasicEnglishPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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

    // Log để debug (Render sẽ hiện cái này trong tab Logs)
    Console.WriteLine($"[DEBUG] Raw ConnectionString: {connectionString}");

    // Kiểm tra xem có phải dạng URL không (chấp nhận cả postgres:// và postgresql://)
    if (!string.IsNullOrEmpty(connectionString) &&
       (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://")))
    {
        try
        {
            var databaseUri = new Uri(connectionString);
            var userInfo = databaseUri.UserInfo.Split(':');

            var builderDb = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
                Database = databaseUri.AbsolutePath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };

            connectionString = builderDb.ToString();
            Console.WriteLine($"[DEBUG] Parsed ConnectionString: Host={builderDb.Host}; Database={builderDb.Database}");
        }
        catch (Exception ex)
        {
            // Ném lỗi ra để biết đường sửa nếu parse sai
            throw new Exception($"Lỗi parse connection string: {ex.Message}");
        }
    }

    // Cấu hình Npgsql và Split Query (để fix warning hiệu năng)
    options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
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
