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

        private async void OnAgendarCitaClicked(object sender, EventArgs e)
        {
            try
            {
                var cliente = await PerfilService.GetClienteActual();

                if (cliente == null)
                {
                    await DisplayAlert("Error", "No se pudo obtener la información del cliente.", "OK");
                    return;
                }

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
            await Navigation.PushAsync(new MisCitasPage());
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool logout = await DisplayAlert("Cerrar Sesión", "¿Estás seguro que deseas cerrar sesión?", "Sí", "No");

            if (logout)
            {
                Preferences.Remove("ClienteId");
                Preferences.Remove("UserId");
                Preferences.Remove("Username");

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