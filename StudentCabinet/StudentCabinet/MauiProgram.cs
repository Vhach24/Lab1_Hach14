using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StudentCabinet.Services;
using Serilog;
using Sentry;

namespace StudentCabinet
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(FileSystem.AppDataDirectory, "logs", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                Log.Fatal(ex, "Необроблений виняток (AppDomain)");
                SentrySdk.CaptureException(ex);
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Log.Fatal(e.Exception, "Необроблений виняток у фоновій задачі (TaskScheduler)");
                SentrySdk.CaptureException(e.Exception);
                e.SetObserved();
            };

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSentry(options =>
                {
                    options.Dsn = "https://e1f2c42eea771901ed8a3c4fbf938296@o4512273035755520.ingest.de.sentry.io/4512273045979216";
                    options.Debug = true;
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Logging.AddSerilog(dispose: true);

            builder.Services.AddSingleton<IAppStateService, AppStateService>();

            builder.Services.AddSingleton<ISyncService>(s =>
                new SyncService(
                    Path.Combine(FileSystem.AppDataDirectory, "students.db3"),
                    s.GetRequiredService<ILogger<SyncService>>()));

            builder.Services.AddSingleton<IStudentRepository>(s =>
                new StudentRepository(
                    Path.Combine(FileSystem.AppDataDirectory, "students.db3"),
                    s.GetRequiredService<ISyncService>(),
                    s.GetRequiredService<ILogger<StudentRepository>>()));

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}