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
    }
}