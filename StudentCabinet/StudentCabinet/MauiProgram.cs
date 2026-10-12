using Microsoft.Extensions.Logging;
using StudentCabinet.Services;

namespace StudentCabinet
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IAppStateService, AppStateService>();

            builder.Services.AddSingleton<ISyncService>(s =>
                new SyncService(Path.Combine(FileSystem.AppDataDirectory, "students.db3")));

            builder.Services.AddSingleton<IStudentRepository>(s =>
                new StudentRepository(Path.Combine(FileSystem.AppDataDirectory, "students.db3"), s.GetRequiredService<ISyncService>()));

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}