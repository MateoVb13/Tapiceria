using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tapiceria.Models;
using Tapiceria.Config;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage; // Para Preferences

namespace Tapiceria.Views
{
    public partial class MisCitasPage : ContentPage
    {
        private int _loggedInUserId;
        private int _loggedInClientId;

        public MisCitasPage()
        {
            InitializeComponent();

            // Obtener el ID del usuario logueado (ajusta según cómo lo almacenes)
            int userId = Preferences.Get("LoggedInUserId", 0); // Ejemplo usando Preferencias
            _loggedInUserId = userId;

            // Al cargar la página, obtener el ID del cliente y luego cargar las citas
            _ = LoadClientIdAndCitasAsync();
        }

        // Método para obtener el ID del cliente y luego cargar las citas
        private async Task LoadClientIdAndCitasAsync()
        {
            if (_loggedInUserId <= 0)
            {
                await DisplayAlert("Error de Sesión", "No se pudo identificar al usuario logueado.", "OK");
                return;
            }

            activityIndicator.IsRunning = true;
            activityIndicator.IsVisible = true;

            try
            {
                using var client = new HttpClient();
                // Endpoint GET en tu API: /api/Clientes/PorUsuario/{idUsuario}
                var urlCliente = $"{ApiConfig.BaseUrl}api/Clientes/PorUsuario/{_loggedInUserId}";

                var responseCliente = await client.GetAsync(urlCliente);

                if (responseCliente.IsSuccessStatusCode)
                {
                    var jsonCliente = await responseCliente.Content.ReadAsStringAsync();
                    var cliente = JsonConvert.DeserializeObject<Cliente>(jsonCliente);

                    if (cliente != null && cliente.IdCliente > 0)
                    {
                        _loggedInClientId = cliente.IdCliente;
                        Console.WriteLine($"ID de Cliente logueado: {_loggedInClientId}");

                        // Ahora que tenemos el IdCliente, cargar las citas
                        await LoadCitasAsync(_loggedInClientId);
                    }
                    else
                    {
                        await DisplayAlert("Error de Cliente", "No se encontró información de cliente para el usuario logueado.", "OK");
                        citasCollectionView.ItemsSource = new List<CitaListItemDto>();
                    }
                }
                else
                {
                    var errorCliente = await responseCliente.Content.ReadAsStringAsync();
                    await DisplayAlert("Error API Cliente", $"Error al obtener ID de cliente: {responseCliente.StatusCode}", "OK");
                    citasCollectionView.ItemsSource = new List<CitaListItemDto>();
                }
            }
            catch (HttpRequestException httpEx)
            {
                await DisplayAlert("Error de Conexión", $"No se pudo conectar al servidor al obtener cliente. Detalles: {httpEx.Message}", "OK");
                citasCollectionView.ItemsSource = new List<CitaListItemDto>();
            }
            catch (JsonException jsonEx)
            {
                await DisplayAlert("Error de Datos", $"Error al procesar los datos del cliente. Detalles: {jsonEx.Message}", "OK");
                citasCollectionView.ItemsSource = new List<CitaListItemDto>();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error Inesperado", $"Ocurrió un error al obtener el cliente: {ex.Message}", "OK");
                citasCollectionView.ItemsSource = new List<CitaListItemDto>();
            }
            finally
            {
                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;
            }
        }

        private async Task LoadCitasAsync(int clientId)
        {
            if (clientId <= 0) return;

            activityIndicator.IsRunning = true;
            activityIndicator.IsVisible = true;

            try
            {
                using var client = new HttpClient();
                // Endpoint GET en tu API: /api/Citas/PorCliente/{id}
                // Debe retornar una lista de CitaListItemDto (o un formato compatible)
                var urlCitas = $"{ApiConfig.BaseUrl}api/Citas/PorCliente/{clientId}";

                var responseCitas = await client.GetAsync(urlCitas);

                if (responseCitas.IsSuccessStatusCode)
                {
                    var jsonCitas = await responseCitas.Content.ReadAsStringAsync();
                    var citasList = JsonConvert.DeserializeObject<List<CitaListItemDto>>(jsonCitas);
                    citasCollectionView.ItemsSource = citasList;
                }
                else
                {
                    var errorCitas = await responseCitas.Content.ReadAsStringAsync();
                    await DisplayAlert("Error API Citas", $"Error al cargar citas: {responseCitas.StatusCode}", "OK");
                    citasCollectionView.ItemsSource = new List<CitaListItemDto>();
                }
            }
            catch (HttpRequestException httpEx)
            {
                await DisplayAlert("Error de Conexión", $"No se pudo conectar al servidor al cargar citas. Detalles: {httpEx.Message}", "OK");
                citasCollectionView.ItemsSource = new List<CitaListItemDto>();
            }
            catch (JsonException jsonEx)
            {
                await DisplayAlert("Error de Datos", $"Error al procesar los datos de citas. Detalles: {jsonEx.Message}", "OK");
                citasCollectionView.ItemsSource = new List<CitaListItemDto>();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error Inesperado", $"Ocurrió un error al cargar citas: {ex.Message}", "OK");
                citasCollectionView.ItemsSource = new List<CitaListItemDto>();
            }
            finally
            {
                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;
            }
        }
    }
}