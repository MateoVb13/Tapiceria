using System;
using Tapiceria.Views;

namespace Tapiceria.Views
{
    public partial class InicioPage : ContentPage
    {
        public InicioPage()
        {
            InitializeComponent();
        }

        private async void OnAgendarCitaClicked(object sender, EventArgs e)
        {
            // Navegar a la nueva página de agendamiento de citas
            await Navigation.PushAsync(new AgendarCitaPage());
        }

        private async void OnVerMisCitasClicked(object sender, EventArgs e)
        {
            // Navegar a la página mejorada de mis citas
            await Navigation.PushAsync(new MisCitasPage());
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool logout = await DisplayAlert("Cerrar Sesión", "¿Estás seguro que deseas cerrar sesión?", "Sí", "No");

            if (logout)
            {
                // Limpiar datos de sesión
                Preferences.Remove("ClienteId");
                Preferences.Remove("UserId");
                Preferences.Remove("Username");

                // Navegar al inicio/login
                Application.Current.MainPage = new NavigationPage(new MainPage());
            }
        }
        private async void OnVerPerfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PerfilPage());
        }
        private async void OnPagosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PagosPage());
        }
    }
}