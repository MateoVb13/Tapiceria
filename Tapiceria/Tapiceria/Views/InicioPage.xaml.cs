using Microsoft.Maui.Controls;
using Tapiceria.Views;

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
            Application.Current.MainPage = new NavigationPage(new MainPage());
            // Limpiar datos de sesión (ej. UserID, ClienteID, Token)
            Microsoft.Maui.Storage.Preferences.Remove("LoggedInUserId");
            // Puedes limpiar más si es necesario
        }

        // Navega a la página central de agendamiento
        private async void OnAgendarCitaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AgendarCitaSimplePage());
        }

        // Navega a la página de mis citas
        private async void OnVerMisCitasClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MisCitasPage());
        }
    }
}