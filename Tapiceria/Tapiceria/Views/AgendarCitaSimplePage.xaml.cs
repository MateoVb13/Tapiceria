using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tapiceria.Models;
using Tapiceria.Config;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using Microsoft.Maui.Storage;

namespace Tapiceria.Views
{
    public partial class AgendarCitaSimplePage : ContentPage
    {
        private List<Servicio> _servicios;
        private Servicio _selectedServicio;
        private TimeSpan _selectedTimeSlot;
        private DateTime _selectedDate;
        private int _loggedInClientId;

        public AgendarCitaSimplePage()
        {
            InitializeComponent();

            SetUIState(false, false, false, false, false); // Ocultar controles inicialmente

            int userId = Preferences.Get("LoggedInUserId", 0);
            if (userId > 0)
            {
                _ = LoadClientIdAsync(userId);
            }
            else
            {
                DisplayAlert("Error de Sesión", "No se pudo identificar al usuario logueado.", "OK");
                Navigation.PopAsync();
                return;
            }

            _ = LoadServiciosAsync();
            datePicker.MinimumDate = DateTime.Today;
        }

        private void SetUIState(bool showDate, bool showTimeLabel, bool showTimeCollection, bool enableConfirm, bool isBusy)
        {
            lblSeleccionaFecha.IsVisible = showDate;
            datePicker.IsVisible = showDate;
            lblHorariosDisponibles.IsVisible = showTimeLabel;
            horariosDisponiblesCollectionView.IsVisible = showTimeCollection;
            btnConfirmarCita.IsEnabled = enableConfirm;
            activityIndicator.IsRunning = isBusy;
            activityIndicator.IsVisible = isBusy;
        }

        private async Task LoadClientIdAsync(int userId)
        {
            if (userId <= 0) return;
            try
            {
                using var client = new HttpClient();
                var urlCliente = $"{ApiConfig.BaseUrl}api/Clientes/PorUsuario/{userId}"; // Endpoint GET en tu API

                var responseCliente = await client.GetAsync(urlCliente);
                if (responseCliente.IsSuccessStatusCode)
                {
                    var jsonCliente = await responseCliente.Content.ReadAsStringAsync();
                    var cliente = JsonConvert.DeserializeObject<Cliente>(jsonCliente);
                    if (cliente != null && cliente.IdCliente > 0)
                    {
                        _loggedInClientId = cliente.IdCliente;
                        Debug.WriteLine($"ID de Cliente logueado: {_loggedInClientId}");
                    }
                    else await DisplayAlert("Error de Cliente", "No se encontró información de cliente para el usuario logueado.", "OK");
                }
                else await DisplayAlert("Error API Cliente", $"Error al obtener ID de cliente: {responseCliente.StatusCode}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al obtener el cliente: {ex.Message}", "OK");
            }
        }

        private async Task LoadServiciosAsync()
        {
            SetUIState(false, false, false, false, true);
            servicePicker.IsEnabled = false;

            try
            {
                using var client = new HttpClient();
                var url = $"{ApiConfig.BaseUrl}api/Servicios"; // Endpoint GET en tu API
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _servicios = JsonConvert.DeserializeObject<List<Servicio>>(json);
                    servicePicker.ItemsSource = _servicios;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Error al cargar servicios: {response.StatusCode}", "OK");
                    _servicios = new List<Servicio>();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al cargar servicios: {ex.Message}", "OK");
                _servicios = new List<Servicio>();
            }
            finally
            {
                SetUIState(false, false, false, false, false);
                servicePicker.IsEnabled = true;
            }
        }

        private void OnServiceSelected(object sender, EventArgs e)
        {
            int selectedIndex = servicePicker.SelectedIndex;

            if (selectedIndex != -1)
            {
                _selectedServicio = _servicios[selectedIndex];
                lblPrecioServicio.Text = _selectedServicio.Precio.ToString("C");

                SetUIState(true, true, true, false, false); // Mostrar fecha y horarios (vacíos)

                _selectedDate = datePicker.Date;
                _ = LoadAndShowAvailableTimeSlotsAsync(_selectedDate);

            }
            else
            {
                _selectedServicio = null;
                lblPrecioServicio.Text = "--";
                SetUIState(false, false, false, false, false);
                horariosDisponiblesCollectionView.ItemsSource = new List<TimeSpan>();
                _selectedTimeSlot = TimeSpan.Zero;
                horariosDisponiblesCollectionView.SelectedItem = null;
            }
        }

        private async void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            _selectedDate = e.NewDate;
            await LoadAndShowAvailableTimeSlotsAsync(_selectedDate);

            _selectedTimeSlot = TimeSpan.Zero;
            horariosDisponiblesCollectionView.SelectedItem = null;
            btnConfirmarCita.IsEnabled = false;
        }

        private async Task LoadAndShowAvailableTimeSlotsAsync(DateTime date)
        {
            if (_selectedServicio == null) return;

            SetUIState(true, true, true, false, true);
            horariosDisponiblesCollectionView.ItemsSource = new List<TimeSpan>();

            try
            {
                using var client = new HttpClient();
                // Endpoint GET en tu API (asumido existente y funcional): /api/Citas/AvailableSlotsForService?idServicio={idServicio}&fecha={fecha}
                // AJUSTA ESTA URL si el endpoint real en tu Swagger es diferente
                var url = $"{ApiConfig.BaseUrl}api/Citas/AvailableSlotsForService?idServicio={_selectedServicio.IdServicio}&fecha={date:yyyy-MM-dd}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    // Asumimos que la API retorna una LISTA de TimeSpans o STRINGS representando las horas
                    var availableSlots = JsonConvert.DeserializeObject<List<TimeSpan>>(json);
                    horariosDisponiblesCollectionView.ItemsSource = availableSlots.OrderBy(t => t).ToList();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Error al cargar horarios disponibles: {response.StatusCode}", "OK");
                    horariosDisponiblesCollectionView.ItemsSource = new List<TimeSpan>();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al cargar horarios: {ex.Message}", "OK");
                horariosDisponiblesCollectionView.ItemsSource = new List<TimeSpan>();
            }
            finally
            {
                SetUIState(true, true, true, btnConfirmarCita.IsEnabled, false); // Mantener visibilidad, actualizar solo busy y enabled
            }
        }

        private void OnHorarioSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedTime = e.CurrentSelection.FirstOrDefault();

            if (selectedTime is TimeSpan selectedTimeSpan)
            {
                _selectedTimeSlot = selectedTimeSpan;
                btnConfirmarCita.IsEnabled = true;
            }
            else
            {
                _selectedTimeSlot = TimeSpan.Zero;
                btnConfirmarCita.IsEnabled = false;
            }
        }

        private async void OnConfirmarCitaClicked(object sender, EventArgs e)
        {
            if (_selectedServicio == null || _selectedDate == DateTime.MinValue || _selectedTimeSlot == TimeSpan.Zero || _loggedInClientId <= 0)
            {
                await DisplayAlert("Error", "Faltan datos para agendar la cita.", "OK");
                return;
            }

            DateTime fechaHoraInicio = _selectedDate.Add(_selectedTimeSlot);

            TimeSpan duracionServicio = TimeSpan.FromMinutes(30); // Asumimos 30 minutos o la duración viene del modelo Servicio
                                                                  // if(_selectedServicio.Duracion > TimeSpan.Zero) duracionServicio = _selectedServicio.Duracion;

            DateTime fechaHoraFin = fechaHoraInicio.Add(duracionServicio);

            var nuevaCita = new CitaCreacionDto
            {
                IdCliente = _loggedInClientId,
                IdServicio = _selectedServicio.IdServicio,
                FechaInicio = fechaHoraInicio,
                FechaFin = fechaHoraFin,
                Estado = "Pendiente",
                Notas = ""
            };

            SetUIState(true, true, true, false, true); // Deshabilitar botón, mostrar indicador
            HttpResponseMessage response = null;

            try
            {
                using var client = new HttpClient();
                var url = $"{ApiConfig.BaseUrl}api/Citas"; // Endpoint POST en tu API

                var json = JsonConvert.SerializeObject(nuevaCita);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Cita agendada correctamente.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Error al agendar cita: {response.StatusCode} - {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al agendar cita: {ex.Message}", "OK");
            }
            finally
            {
                SetUIState(true, true, true, btnConfirmarCita.IsEnabled, false); // Ocultar indicador
                if (response != null && !response.IsSuccessStatusCode)
                {
                    btnConfirmarCita.IsEnabled = true; // Re-habilitar si falló la API
                }
            }
        }
    }
}