using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Photino.Blazor;
using SnapshotDiff.Features.Config;
using SnapshotDiff.Features.ExclusionRules.Infrastructure;
using SnapshotDiff.Features.Export.Infrastructure;
using SnapshotDiff.Features.Scanner.Infrastructure;
using SnapshotDiff.Features.Trash.Infrastructure;
using SnapshotDiff.Infrastructure;
using SnapshotDiff.Infrastructure.Storage;
using SnapshotDiff.Linux.Services;

var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);

// Linux storage path provider
appBuilder.Services.AddSingleton<IStoragePathProvider, LinuxStoragePathProvider>();

// Linux folder picker using zenity/kdialog
appBuilder.Services.AddSingleton<IFolderPickerService, LinuxFolderPickerService>();

// Shared core services
appBuilder.Services.AddInfrastructure();
appBuilder.Services.AddConfig();
appBuilder.Services.AddScanner();
appBuilder.Services.AddExport();
appBuilder.Services.AddLocalization();
appBuilder.Services.AddTrash();
appBuilder.Services.AddExclusionRules();

// Culture service
appBuilder.Services.AddScoped<SnapshotDiff.Infrastructure.Localization.ICultureService,
    LinuxCultureService>();

appBuilder.Services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));

appBuilder.RootComponents.Add<SnapshotDiff.Linux.Components.Routes>("#app");

var app = appBuilder.Build();

app.MainWindow
    .SetTitle("SnapshotDiff")
    .SetSize(1280, 800)
    .SetResizable(true)
    .SetIconFile("appicon.png");

AppDomain.CurrentDomain.UnhandledException += (_, args) =>
{
    Console.Error.WriteLine($"Unhandled exception: {args.ExceptionObject}");
};

app.Run();