using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tapiceria.Models;
using Tapiceria.Services;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;

namespace Tapiceria.Views
{
    public partial class AgendarCitaPage : ContentPage
    {
        private readonly CitasService _citasService;
        private Servicio _servicioSeleccionado;
        private DisponibilidadHoraria _horarioSeleccionado;
        private int _clienteId;
        private List<Button> _horariosButtons = new List<Button>();

        // Propiedad para la fecha mínima (hoy)
        public DateTime MinDate => DateTime.Today;

        public AgendarCitaPage()
        {
            InitializeComponent();

            // Inicializar servicio de citas
            _citasService = new CitasService();

            // Configurar fecha mínima y la fecha por defecto como hoy
            datePicker.MinimumDate = MinDate;
            datePicker.Date = MinDate;

            // Establecer el contexto de vinculación para MinDate
            this.BindingContext = this;

            // Cargar el ID del cliente desde Preferences
            _clienteId = Preferences.Get("ClienteId", 0);

            // Cargar los servicios disponibles al iniciar
            LoadServicios();
        }

        private async void LoadServicios()
        {
            try
            {
                activityIndicator.IsRunning = true;

                // Cargar servicios en un hilo secundario
                List<Servicio> servicios = null;
                await Task.Run(async () => {
                    servicios = await _citasService.GetServiciosAsync();
                });

                MainThread.BeginInvokeOnMainThread(() => {
                    if (servicios != null && servicios.Any())
                    {
                        servicePicker.ItemsSource = servicios;
                    }
                    else
                    {
                        DisplayAlert("Sin servicios", "No se encontraron servicios disponibles", "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar los servicios: {ex.Message}", "OK");
            }
            finally
            {
                activityIndicator.IsRunning = false;
            }
        }

        private async void OnServiceSelected(object sender, EventArgs e)
        {
            ClearHorarioSelection();

            if (servicePicker.SelectedItem is Servicio servicio)
            {
                _servicioSeleccionado = servicio;

                // Actualizar información del servicio
                lblDuracion.Text = $"{servicio.DuracionEstimada} minutos";
                lblPrecio.Text = $"${servicio.Precio:N2}";

                // Si ya hay una fecha seleccionada, cargar los horarios
                if (datePicker.Date != null)
                {
                    // Usar un pequeño retraso para evitar bloqueos de UI
                    await Task.Delay(100);
                    await LoadHorariosDisponibles();
                }
            }
            else
            {
                // Limpiar información si no hay servicio seleccionado
                lblDuracion.Text = "--";
                lblPrecio.Text = "--";
                ClearHorarios();
            }

            UpdateDetallesCita();
            UpdateConfirmButtonState();
        }

        private async void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            ClearHorarioSelection();

            // Si ya hay un servicio seleccionado, cargar los horarios
            if (_servicioSeleccionado != null)
            {
                // Usar un pequeño retraso para evitar bloqueos de UI
                await Task.Delay(100);
                await LoadHorariosDisponibles();
            }

            UpdateDetallesCita();
            UpdateConfirmButtonState();
        }

        private async Task LoadHorariosDisponibles()
        {
            if (_servicioSeleccionado == null) return;

            try
            {
                // Mostrar indicador de actividad
                activityIndicator.IsRunning = true;

                // Limpiar selección anterior
                ClearHorarioSelection();
                ClearHorarios();

                // Obtener horarios disponibles de la API de forma asíncrona y en segundo plano
                List<DisponibilidadHoraria> horarios = null;
                await Task.Run(async () => {
                    horarios = await _citasService.GetHorariosDisponiblesAsync(
                        _servicioSeleccionado.IdServicio,
                        datePicker.Date);
                });

                // Actualizar la visualización de horarios en el hilo de UI
                MainThread.BeginInvokeOnMainThread(() => {
                    if (horarios != null && horarios.Any())
                    {
                        lblNoHorarios.IsVisible = false;
                        MostrarHorariosDisponibles(horarios);
                    }
                    else
                    {
                        lblNoHorarios.IsVisible = true;
                    }
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar los horarios: {ex.Message}", "OK");
                lblNoHorarios.IsVisible = true;
            }
            finally
            {
                // Ocultar indicador de actividad
                activityIndicator.IsRunning = false;
            }
        }

        private void MostrarHorariosDisponibles(List<DisponibilidadHoraria> horarios)
        {
            // Ejecutar en el hilo de UI para evitar bloqueos
            MainThread.BeginInvokeOnMainThread(() => {
                // Limpiamos los botones de horarios actuales
                ClearHorarios();

                // Agrupamos horarios por hora
                var horariosAgrupados = horarios
                    .OrderBy(h => h.FechaInicio.TimeOfDay)
                    .GroupBy(h => h.FechaInicio.TimeOfDay)
                    .ToList();

                int row = 0;
                int column = 0;

                // Limitamos a 5 filas para evitar un grid demasiado largo
                int maxRows = 5;

                // Procesar en lotes pequeños para evitar bloqueos en la UI
                var horariosVisibles = horariosAgrupados.Take(maxRows * 2).ToList(); // Máximo 10 horarios (5 filas x 2 columnas)
                foreach (var grupo in horariosVisibles)
                {
                    // Tomamos el primer horario disponible del grupo
                    var horario = grupo.First();

                    // Creamos un botón para este horario
                    var button = new Button
                    {
                        Text = horario.FechaInicio.ToString("hh:mm tt"),
                        BackgroundColor = Color.FromArgb("#2d2d2d"),
                        TextColor = Colors.White,
                        CornerRadius = 8,
                        HorizontalOptions = LayoutOptions.Fill
                    };

                    // Guardamos la referencia al horario en el CommandParameter
                    button.CommandParameter = horario;

                    // Agregamos el evento click
                    button.Clicked += OnHorarioButtonClicked;

                    // Agregamos al grid en la posición correspondiente
                    gridHorarios.Add(button, column, row);

                    // Agregamos el botón a nuestra lista de control
                    _horariosButtons.Add(button);

                    // Pasamos a la siguiente columna/fila
                    column++;
                    if (column > 1)
                    {
                        column = 0;
                        row++;
                    }
                }

                // Si hay más horarios de los que podemos mostrar, indicarlo
                if (horariosAgrupados.Count > maxRows * 2)
                {
                    var masHorariosLabel = new Label
                    {
                        Text = $"Y {horariosAgrupados.Count - (maxRows * 2)} horarios más disponibles",
                        TextColor = Color.FromArgb("#cccccc"),
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    };

                    // Agregamos al grid en una nueva fila
                    if (row >= maxRows)
                    {
                        // Si ya llegamos al máximo de filas, añadimos una nueva
                        gridHorarios.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }

                    gridHorarios.Add(masHorariosLabel, 0, row, 2, 1); // Span de 2 columnas
                }
            });
        }

        private void OnHorarioButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is DisponibilidadHoraria horario)
            {
                // Limpiar selección anterior
                ClearHorarioSelection();

                // Seleccionar este horario
                _horarioSeleccionado = horario;
                button.BackgroundColor = Color.FromArgb("#d9a520");

                // Actualizar detalles de la cita
                UpdateDetallesCita();
                UpdateConfirmButtonState();
            }
        }

        private void ClearHorarioSelection()
        {
            _horarioSeleccionado = null;

            // Restaurar el color de todos los botones
            foreach (var button in _horariosButtons)
            {
                button.BackgroundColor = Color.FromArgb("#2d2d2d");
            }
        }

        private void ClearHorarios()
        {
            // Ejecutar en el hilo principal para evitar problemas
            MainThread.BeginInvokeOnMainThread(() => {
                // Limpiar botones anteriores
                foreach (var button in _horariosButtons)
                {
                    gridHorarios.Remove(button);
                    button.Clicked -= OnHorarioButtonClicked;
                }

                // Limpiar la colección
                _horariosButtons.Clear();

                // Limpiar otros elementos
                foreach (var child in gridHorarios.Children.ToList())
                {
                    gridHorarios.Remove(child);
                }
            });
        }

        private void UpdateDetallesCita()
        {
            // Si tenemos servicio y horario seleccionados, mostrar detalles
            if (_servicioSeleccionado != null && _horarioSeleccionado != null)
            {
                lblServicioSeleccionado.Text = _servicioSeleccionado.Descripcion;
                lblFechaSeleccionada.Text = datePicker.Date.ToString("D");
                lblHoraSeleccionada.Text = _horarioSeleccionado.FechaInicio.ToString("hh:mm tt");
                lblEmpleadoSeleccionado.Text = _horarioSeleccionado.NombreEmpleado;

                frameCitaSeleccionada.IsVisible = true;
            }
            else
            {
                frameCitaSeleccionada.IsVisible = false;
            }
        }

        private void UpdateConfirmButtonState()
        {
            // Habilitar el botón solo si se han seleccionado servicio y horario
            btnConfirmar.IsEnabled = (_servicioSeleccionado != null && _horarioSeleccionado != null);
        }

        private async void OnConfirmarClicked(object sender, EventArgs e)
        {
            if (_servicioSeleccionado == null || _horarioSeleccionado == null)
            {
                await DisplayAlert("Información incompleta", "Por favor selecciona un servicio y un horario disponible.", "OK");
                return;
            }

            // Mostrar confirmación
            bool confirmar = await DisplayAlert("Confirmar cita",
                $"¿Deseas confirmar tu cita para {_servicioSeleccionado.Descripcion} el {datePicker.Date.ToString("D")} a las {_horarioSeleccionado.FechaInicio.ToString("hh:mm tt")}?",
                "Confirmar", "Cancelar");

            if (!confirmar) return;

            try
            {
                activityIndicator.IsRunning = true;
                btnConfirmar.IsEnabled = false;

                // Crear objeto DTO para la cita
                var nuevaCita = new CitaCreacionDto
                {
                    IdCliente = _clienteId,
                    IdServicio = _servicioSeleccionado.IdServicio,
                    IdEmpleado = _horarioSeleccionado.IdEmpleado,
                    FechaInicio = _horarioSeleccionado.FechaInicio,
                    FechaFin = _horarioSeleccionado.FechaFin,
                    Estado = "Pendiente",
                    Notas = string.IsNullOrWhiteSpace(notasEditor.Text) ? null : notasEditor.Text
                };

                // Enviar la cita a la API en segundo plano
                var citaCreada = false;
                await Task.Run(async () => {
                    citaCreada = await _citasService.CreateCitaAsync(nuevaCita) != null;
                });

                if (citaCreada)
                {
                    await DisplayAlert("¡Cita Agendada!",
                        "Tu cita ha sido agendada exitosamente. Recibirás una confirmación pronto.\n\n" +
                        $"Servicio: {_servicioSeleccionado.Descripcion}\n" +
                        $"Fecha: {datePicker.Date.ToString("D")}\n" +
                        $"Hora: {_horarioSeleccionado.FechaInicio.ToString("hh:mm tt")}\n" +
                        $"Tapicero: {_horarioSeleccionado.NombreEmpleado}",
                        "OK");

                    // Volver a la página anterior
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo agendar la cita: {ex.Message}", "OK");
            }
            finally
            {
                activityIndicator.IsRunning = false;
                btnConfirmar.IsEnabled = true;
            }
        }

        // Método que maneja la carga de datos pesados con mejor desempeño
        private async Task<List<T>> ExecuteWithoutBlockingUI<T>(Func<Task<List<T>>> asyncOperation)
        {
            List<T> result = null;

            // Usar Task.Run para mover la operación a un hilo secundario
            await Task.Run(async () =>
            {
                try
                {
                    result = await asyncOperation();
                }
                catch (Exception ex)
                {
                    // Capturar excepciones para manejarlas después
                    Console.WriteLine($"Error en operación asíncrona: {ex.Message}");
                    result = new List<T>();
                }
            });

            return result;
        }
    }
}