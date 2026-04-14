using Serilog;
using SnapshotDiff.Components;
using SnapshotDiff.Features.Config;
using SnapshotDiff.Features.ExclusionRules.Infrastructure;
using SnapshotDiff.Features.Export.Infrastructure;
using SnapshotDiff.Features.Scanner.Infrastructure;
using SnapshotDiff.Features.Trash.Infrastructure;
using SnapshotDiff.Infrastructure;
using SnapshotDiff.Infrastructure.Localization;
using SnapshotDiff.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/web-.log", rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7, fileSizeLimitBytes: 10 * 1024 * 1024)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Core services
builder.Services.AddInfrastructure();
builder.Services.AddConfig();
builder.Services.AddScanner();
builder.Services.AddExport();
builder.Services.AddLocalization();
builder.Services.AddTrash();
builder.Services.AddExclusionRules();

// Web host culture switcher
builder.Services.AddScoped<ICultureService, WebCultureService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SnapshotDiff.Features.Scanner.UI.Pages.ScanPage).Assembly);

app.Run();