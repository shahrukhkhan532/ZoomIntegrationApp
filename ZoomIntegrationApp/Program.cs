using ZoomIntegrationApp.Models;
using ZoomIntegrationApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddTransient<IMeetingService, MeetingService>();
builder.Services.AddTransient<IFiles, Files>();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
ConfigurationManager configuration = builder.Configuration;
builder.Services.Configure<Zoom>(configuration.GetSection("Zoom"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
