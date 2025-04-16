namespace Tapiceria
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new Views.LoginPage();
        }
    }
}
