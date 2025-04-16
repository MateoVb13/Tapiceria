namespace Tapiceria.Views
{
    public partial class InicioPage : ContentPage
    {
        public InicioPage()
        {
            InitializeComponent();
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            // Cierra sesión y vuelve a MainPage como pantalla inicial
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
    }
}
