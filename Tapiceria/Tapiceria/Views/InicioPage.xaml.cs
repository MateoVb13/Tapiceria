using System;
using Tapiceria.Views;
using Tapiceria.Services;

namespace Tapiceria.Views
{
    public partial class InicioPage : ContentPage
    {
        public InicioPage()
        {
            InitializeComponent();
        }

        // En Tapiceria/Views/InicioPage.xaml.cs - Modificar el método OnAgendarCitaClicked
        private async void OnAgendarCitaClicked(object sender, EventArgs e)
        {
            try
            {
                // Obtener el cliente actual
                var cliente = await PerfilService.GetClienteActual();

                if (cliente == null)
                {
                    await DisplayAlert("Error", "No se pudo obtener la información del cliente.", "OK");
                    return;
                }

                // Verificar si tiene citas pendientes
                var citasService = new Services.CitasService();
                bool tieneCitasPendientes = await citasService.TieneCitasPendientesAsync(cliente.IdCliente);

                if (tieneCitasPendientes)
                {
                    await DisplayAlert("Restricción",
                        "Tienes una cita pendiente. No puedes agendar nuevas citas hasta que confirmes o canceles la cita pendiente.",
                        "Entendido");
                    return;
                }


                await Navigation.PushAsync(new AgendarCitaPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al verificar disponibilidad: {ex.Message}", "OK");
            }
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