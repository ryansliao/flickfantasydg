using fantasydg.Data;
using fantasydg.Models;
using fantasydg.Models.Repository;
using fantasydg.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Options;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddUserSecrets<Program>();
    var configuration = builder.Configuration;

    builder.Services.AddAuthentication().AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
    });

    // Add logging (built-in, no extra config needed)
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();

    // Add services to the container.
    //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        )
    );

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.AllowedUserNameCharacters = null; // ? allows everything valid in email
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddControllersWithViews();

    builder.Services.AddHttpClient<DataService>();
    builder.Services.AddScoped<DatabaseRepository>();
    builder.Services.AddScoped<LeagueService>();
    builder.Services.AddScoped<TournamentDiscoveryService>();
    builder.Services.AddHostedService<TournamentDiscoveryService>();
    builder.Services.AddScoped<TournamentUpdateService>();
    builder.Services.AddHostedService<TournamentUpdateService>();
    builder.Services.AddScoped<PlayerService>();

    var app = builder.Build();

    app.UseDeveloperExceptionPage();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    throw;
}