using StudentCabinet.Models;
using StudentCabinet.ViewModels;

namespace StudentCabinet.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnDetailsClicked(object sender, EventArgs e)
        {
            if (sender is Button button &&
                button.BindingContext is Student student &&
                BindingContext is StudentViewModel vm)
            {
                vm.OpenDetailsCommand.Execute(student);
            }
        }
    }
}