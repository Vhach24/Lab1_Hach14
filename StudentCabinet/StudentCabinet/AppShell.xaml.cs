using StudentCabinet.Views;

namespace StudentCabinet
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("studentdetail", typeof(StudentDetailPage));
        }
    }
}