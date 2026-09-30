using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using StudentCabinet.Services;

namespace StudentCabinet.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly IAppStateService _appState =
            IPlatformApplication.Current?.Services.GetService<IAppStateService>()
            ?? new AppStateService();

        private bool _isNavigating;

        private bool _isDarkTheme;

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (_isDarkTheme != value)
                {
                    _isDarkTheme = value;
                    OnPropertyChanged();

                    Application.Current!.UserAppTheme = value ? AppTheme.Dark : AppTheme.Light;
                    Preferences.Set("app_theme", value ? "Dark" : "Light");
                }
            }
        }

        private string _deepLinkStudentIdText = string.Empty;

        public string DeepLinkStudentIdText
        {
            get => _deepLinkStudentIdText;
            set
            {
                if (_deepLinkStudentIdText != value)
                {
                    _deepLinkStudentIdText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OpenDeepLinkCommand { get; }

        public SettingsViewModel()
        {
            var savedTheme = Preferences.Get("app_theme", "System");
            _isDarkTheme = savedTheme == "Dark";

            OpenDeepLinkCommand = new Command(async () => await OpenDeepLinkAsync());
        }

        private async Task OpenDeepLinkAsync()
        {
            if (_isNavigating) return;
            if (!int.TryParse(DeepLinkStudentIdText, out var id)) return;

            _isNavigating = true;
            try
            {
                _appState.PendingDeepLinkStudentId = id;
                await NavigateBackSafeAsync();
            }
            finally
            {
                _isNavigating = false;
            }
        }

        private async Task NavigateBackSafeAsync()
        {
            for (int attempt = 0; attempt < 6; attempt++)
            {
                try
                {
                    await Shell.Current.GoToAsync("..");
                    return;
                }
                catch (InvalidOperationException)
                {
                    await Task.Delay(250);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}