using StudentCabinet.ViewModels;

namespace StudentCabinet.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is StudentViewModel vm)
            {
                vm.RestoreDraftState();
                vm.LoadCommand.Execute(null);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (BindingContext is StudentViewModel vm)
            {
                vm.SaveDraftState();
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("settings");
        }

        private void OnSimulateCrashClicked(object sender, EventArgs e)
        {
            Task.Run(() => throw new InvalidOperationException("Тестовий необроблений виняток у фоновій задачі"));
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private async void OnViewLogsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("logviewer");
        }
    }
}