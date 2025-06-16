using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tapiceria.Models;
using Tapiceria.Services;
using Newtonsoft.Json;

namespace Tapiceria.Views
{
    public partial class MisCitasPage : ContentPage
    {
        private readonly CitasService _citasService;
        private int _idCliente;

        public MisCitasPage()
        {
            InitializeComponent();
            _citasService = new CitasService();
            LoadCitas();
        }

        private async void LoadCitas()
        {
            try
            {
                activityIndicator.IsRunning = true;
                activityIndicator.IsVisible = true;

                var clienteActual = await PerfilService.GetClienteActual();
                if (clienteActual != null)
                {
                    _idCliente = clienteActual.IdCliente;

                    var citas = await _citasService.GetCitasByClienteIdAsync(_idCliente);
                    citasCollectionView.ItemsSource = citas;
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo obtener la información del cliente", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar las citas: {ex.Message}", "OK");
            }
            finally
            {
                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;
            }
        }

        private async void OnCancelarCitaClicked(object sender, EventArgs e)
        {
            try
            {
                var button = sender as Button;
                var cita = button?.CommandParameter as CitaListItemDto;

                if (cita == null)
                {
                    await DisplayAlert("Error", "No se pudo obtener la información de la cita", "OK");
                    return;
                }

                bool confirmar = await DisplayAlert(
                    "Cancelar Cita",
                    $"¿Estás seguro de que quieres cancelar la cita de {cita.NombreServicio} programada para el {cita.Fecha}?",
                    "Sí, Cancelar",
                    "No");

                if (!confirmar)
                    return;

                activityIndicator.IsRunning = true;
                activityIndicator.IsVisible = true;

                bool exitoso = await _citasService.CancelarCitaAsync(cita.IdCita);

                if (exitoso)
                {
                    await DisplayAlert("Éxito", "La cita ha sido cancelada correctamente", "OK");


                    LoadCitas();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo cancelar la cita. Por favor, intenta nuevamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cancelar la cita: {ex.Message}", "OK");
            }
            finally
            {
                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;
            }
        }

        private async void OnNuevaCitaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AgendarCitaPage());
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadCitas();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            LoadCitas();
        }
    }
}