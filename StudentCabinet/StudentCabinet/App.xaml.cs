using System.Web;
using StudentCabinet.Services;

namespace StudentCabinet
{

    public partial class App : Application
    {
        private readonly IAppStateService _appState;

        public App(IAppStateService appState)
        {
            InitializeComponent();

            _appState = appState;

            // Частина 2, п.5: застосувати раніше збережену тему при старті застосунку.
            var savedTheme = Preferences.Get("app_theme", "System");
            UserAppTheme = savedTheme switch
            {
                "Dark" => AppTheme.Dark,
                "Light" => AppTheme.Light,
                _ => AppTheme.Unspecified
            };

            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            // ----- Лабораторна №3, Частина 1, п.1: логування подій життєвого циклу -----

            window.Stopped += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[Lifecycle] Window.Stopped (застосунок згорнуто у фон)");
            };

            window.Resumed += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[Lifecycle] Window.Resumed (застосунок повернувся на передній план)");
            };

            window.Destroying += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[Lifecycle] Window.Destroying (застосунок закривається)");
            };

            return window;
        }

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);

            System.Diagnostics.Debug.WriteLine($"[DeepLink] Отримано посилання: {uri}");

            var query = HttpUtility.ParseQueryString(uri.Query);
            var idText = query.Get("id");

            if (int.TryParse(idText, out var studentId))
            {
                _appState.PendingDeepLinkStudentId = studentId;
            }
        }
    }
}