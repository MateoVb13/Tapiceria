using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Tapiceria.Models;
using Tapiceria.Services;

namespace Tapiceria.Views
{
    public partial class PagosPage : ContentPage
    {
        private PagosService _pagosService;
        private int _clienteId;

        public PagosPage()
        {
            InitializeComponent();
            _pagosService = new PagosService();
            _clienteId = Preferences.Get("ClienteId", 0);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_clienteId <= 0)
            {
                await DisplayAlert("Error", "No se pudo obtener la información del cliente.", "Aceptar");
                await Navigation.PopAsync();
                return;
            }

            await CargarPagosPendientes();
            await CargarHistorialPagos();
        }

        private async Task CargarPagosPendientes()
        {
            try
            {
                activityIndicatorPendientes.IsRunning = true;

                var pagosPendientes = await _pagosService.GetPagosPendientesAsync(_clienteId);

                if (pagosPendientes.Count == 0)
                {
                    lblSinPagosPendientes.IsVisible = true;
                    pagosPendientesCollection.IsVisible = false;
                }
                else
                {
                    lblSinPagosPendientes.IsVisible = false;
                    pagosPendientesCollection.IsVisible = true;
                    pagosPendientesCollection.ItemsSource = pagosPendientes;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando pagos pendientes: {ex.Message}");
                await DisplayAlert("Error", $"Error al cargar pagos pendientes: {ex.Message}", "Aceptar");
            }
            finally
            {
                activityIndicatorPendientes.IsRunning = false;
            }
        }

        private async Task CargarHistorialPagos()
        {
            try
            {
                activityIndicatorHistorial.IsRunning = true;

                var historialPagos = await _pagosService.GetHistorialPagosAsync(_clienteId);

                if (historialPagos.Count == 0)
                {
                    lblSinHistorial.IsVisible = true;
                    historialPagosCollection.IsVisible = false;
                }
                else
                {
                    lblSinHistorial.IsVisible = false;
                    historialPagosCollection.IsVisible = true;
                    historialPagosCollection.ItemsSource = historialPagos;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando historial: {ex.Message}");
                await DisplayAlert("Error", $"Error al cargar historial: {ex.Message}", "Aceptar");
            }
            finally
            {
                activityIndicatorHistorial.IsRunning = false;
            }
        }

        private async void OnPagarClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is PagoDto pago)
            {
                await ProcesarPagoSimulado(pago);
            }
        }

        private async Task ProcesarPagoSimulado(PagoDto pago)
        {
            try
            {
                string tipoPago = await DisplayActionSheet(
                    "Selecciona método de pago",
                    "Cancelar",
                    null,
                    "Efectivo",
                    "Tarjeta de Crédito",
                    "Transferencia Bancaria");

                if (tipoPago == "Cancelar" || string.IsNullOrEmpty(tipoPago))
                    return;

                bool confirmar = await DisplayAlert(
                    "Confirmar Pago",
                    $"¿Confirmas el pago de {pago.MontoString} por {tipoPago} para el servicio {pago.NombreServicio}?",
                    "Confirmar",
                    "Cancelar");

                if (!confirmar)
                    return;

                await DisplayAlert("Procesando...", "Procesando tu pago...", "OK");
                await Task.Delay(2000);

                bool exito = await _pagosService.ProcesarPagoSimuladoAsync(
                    pago.IdCita,
                    tipoPago,
                    pago.MontoAPagar);

                if (exito)
                {
                    await DisplayAlert(
                        "Pago Exitoso",
                        $"Tu pago de {pago.MontoString} ha sido procesado correctamente.\n\nMétodo: {tipoPago}\nServicio: {pago.NombreServicio}",
                        "Aceptar");

                    await CargarPagosPendientes();
                    await CargarHistorialPagos();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo procesar el pago. Por favor, intenta nuevamente.", "Aceptar");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error procesando pago: {ex.Message}", "Aceptar");
            }
        }
    }
}