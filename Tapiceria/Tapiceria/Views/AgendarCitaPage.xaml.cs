using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tapiceria.Models;
using Tapiceria.Services;

namespace Tapiceria.Views
{
    public partial class AgendarCitaPage : ContentPage
    {
        private readonly CitasService _citasService;
        private Servicio _servicioSeleccionado;
        private DisponibilidadHoraria _horarioSeleccionado;
        private int _clienteId;
        private List<Button> _horariosButtons = new List<Button>();
        private PerfilService _perfilService = new PerfilService();
        private Cliente _clienteActual;
        private bool _datosClienteCompletos = false;

        public DateTime MinDate => DateTime.Today;

        public AgendarCitaPage()
        {
            InitializeComponent();

            _citasService = new CitasService();

            datePicker.MinimumDate = MinDate;
            datePicker.Date = MinDate;

            this.BindingContext = this;

            _clienteId = Preferences.Get("ClienteId", 0);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await VerificarDatosCliente();

            if (_datosClienteCompletos)
            {
                LoadServicios();
            }
        }

        private async Task VerificarDatosCliente()
        {
            try
            {
                string userJson = Preferences.Get("usuario_actual", string.Empty);
                int userId = Preferences.Get("UserId", 0);
                int clienteId = Preferences.Get("ClienteId", 0);

                System.Diagnostics.Debug.WriteLine($"Verificando datos cliente:");
                System.Diagnostics.Debug.WriteLine($"   - JSON Usuario: {(!string.IsNullOrEmpty(userJson) ? "Presente" : "Ausente")}");
                System.Diagnostics.Debug.WriteLine($"   - UserId: {userId}");
                System.Diagnostics.Debug.WriteLine($"   - ClienteId: {clienteId}");

                UserDto usuario = null;

                if (!string.IsNullOrEmpty(userJson))
                {
                    try
                    {
                        usuario = JsonConvert.DeserializeObject<UserDto>(userJson);
                        System.Diagnostics.Debug.WriteLine($"Usuario deserializado: ID={usuario?.IdUsuario}");
                    }
                    catch (Exception jsonEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error deserializando JSON: {jsonEx.Message}");
                    }
                }

                if (usuario == null && userId > 0)
                {
                    usuario = new UserDto { IdUsuario = userId };
                    System.Diagnostics.Debug.WriteLine($"Usuario creado con UserId: {userId}");
                }

                if (usuario == null || usuario.IdUsuario <= 0)
                {
                    await DisplayAlert("Sesión inválida", "No se pudo obtener la información del usuario. Por favor, inicia sesión nuevamente.", "Aceptar");
                    await Navigation.PopAsync();
                    return;
                }

                Cliente cliente = null;

                if (clienteId > 0)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Intentando obtener cliente por ID: {clienteId}");
                        cliente = await _perfilService.GetClienteByIdAsync(clienteId);
                        if (cliente != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Cliente obtenido por ID");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error obteniendo cliente por ID: {ex.Message}");
                    }
                }

                if (cliente == null)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Intentando obtener cliente por IdUsuario: {usuario.IdUsuario}");
                        cliente = await _perfilService.GetClienteByUsuarioIdAsync(usuario.IdUsuario);
                        if (cliente != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Cliente obtenido por IdUsuario");
                            Preferences.Set("ClienteId", cliente.IdCliente);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error obteniendo cliente por IdUsuario: {ex.Message}");
                    }
                }

                if (cliente == null)
                {
                    await DisplayAlert("Perfil incompleto",
                        "No se encontró tu perfil de cliente. Por favor, ve a 'Mi Perfil' para completar tu información.",
                        "Aceptar");
                    await Navigation.PopAsync();
                    return;
                }

                _clienteActual = cliente;
                _clienteId = cliente.IdCliente;

                _datosClienteCompletos = !string.IsNullOrWhiteSpace(cliente.Contacto) &&
                                        !string.IsNullOrWhiteSpace(cliente.Direccion);

                System.Diagnostics.Debug.WriteLine($"Estado de los datos:");
                System.Diagnostics.Debug.WriteLine($"   - Cliente ID: {cliente.IdCliente}");
                System.Diagnostics.Debug.WriteLine($"   - Contacto: '{cliente.Contacto}'");
                System.Diagnostics.Debug.WriteLine($"   - Dirección: '{cliente.Direccion}'");
                System.Diagnostics.Debug.WriteLine($"   - Datos completos: {_datosClienteCompletos}");

                if (!_datosClienteCompletos)
                {
                    bool irAPerfil = await DisplayAlert(
                        "Información incompleta",
                        "Para agendar una cita, debes completar tu información de contacto y dirección en tu perfil.",
                        "Completar ahora", "Cancelar");

                    if (irAPerfil)
                    {
                        await Navigation.PushAsync(new PerfilPage());
                    }
                    else
                    {
                        await Navigation.PopAsync();
                    }
                }
                else
                {
                    Preferences.Set("datos_cliente_completos", true);
                    Preferences.Set("ClienteId", cliente.IdCliente);

                    System.Diagnostics.Debug.WriteLine("Datos del cliente verificados correctamente");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en VerificarDatosCliente: {ex.Message}");
                await DisplayAlert("Error", $"No se pudo verificar la información: {ex.Message}", "Aceptar");
                await Navigation.PopAsync();
            }
        }
        private async void LoadServicios()
        {
            try
            {
                activityIndicator.IsRunning = true;

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

                lblDuracion.Text = $"{servicio.DuracionEstimada} minutos";
                lblPrecio.Text = $"${servicio.Precio:N2}";

                if (datePicker.Date != null)
                {
                    await Task.Delay(100);
                    await LoadHorariosDisponibles();
                }
            }
            else
            {
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

            if (_servicioSeleccionado != null)
            {
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
                activityIndicator.IsRunning = true;

                ClearHorarioSelection();
                ClearHorarios();

                List<DisponibilidadHoraria> horarios = null;
                await Task.Run(async () => {
                    horarios = await _citasService.GetHorariosDisponiblesAsync(
                        _servicioSeleccionado.IdServicio,
                        datePicker.Date);
                });

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
                activityIndicator.IsRunning = false;
            }
        }

        private void MostrarHorariosDisponibles(List<DisponibilidadHoraria> horarios)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                ClearHorarios();

                var horariosAgrupados = horarios
                    .OrderBy(h => h.FechaInicio.TimeOfDay)
                    .GroupBy(h => h.FechaInicio.TimeOfDay)
                    .ToList();

                int row = 0;
                int column = 0;

                int maxRows = 5;

                var horariosVisibles = horariosAgrupados.Take(maxRows * 2).ToList();
                foreach (var grupo in horariosVisibles)
                {
                    var horario = grupo.First();

                    var button = new Button
                    {
                        Text = horario.FechaInicio.ToString("hh:mm tt"),
                        BackgroundColor = Color.FromArgb("#2d2d2d"),
                        TextColor = Colors.White,
                        CornerRadius = 8,
                        HorizontalOptions = LayoutOptions.Fill
                    };

                    button.CommandParameter = horario;

                    button.Clicked += OnHorarioButtonClicked;

                    gridHorarios.Add(button, column, row);

                    _horariosButtons.Add(button);


                    column++;
                    if (column > 1)
                    {
                        column = 0;
                        row++;
                    }
                }

                if (horariosAgrupados.Count > maxRows * 2)
                {
                    var masHorariosLabel = new Label
                    {
                        Text = $"Y {horariosAgrupados.Count - (maxRows * 2)} horarios más disponibles",
                        TextColor = Color.FromArgb("#cccccc"),
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    };

                    if (row >= maxRows)
                    {
                        gridHorarios.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }

                    gridHorarios.Add(masHorariosLabel, 0, row, 2, 1);
                }
            });
        }

        private void OnHorarioButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is DisponibilidadHoraria horario)
            {
                ClearHorarioSelection();

                _horarioSeleccionado = horario;
                button.BackgroundColor = Color.FromArgb("#d9a520");


                UpdateDetallesCita();
                UpdateConfirmButtonState();
            }
        }

        private void ClearHorarioSelection()
        {
            _horarioSeleccionado = null;

            foreach (var button in _horariosButtons)
            {
                button.BackgroundColor = Color.FromArgb("#2d2d2d");
            }
        }

        private void ClearHorarios()
        {
            MainThread.BeginInvokeOnMainThread(() => {
                foreach (var button in _horariosButtons)
                {
                    gridHorarios.Remove(button);
                    button.Clicked -= OnHorarioButtonClicked;
                }

                _horariosButtons.Clear();

                foreach (var child in gridHorarios.Children.ToList())
                {
                    gridHorarios.Remove(child);
                }
            });
        }

        private void UpdateDetallesCita()
        {
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
            btnConfirmar.IsEnabled = (_servicioSeleccionado != null && _horarioSeleccionado != null);
        }

        private async void OnConfirmarClicked(object sender, EventArgs e)
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
                        "Se detectó una cita pendiente. No puedes agendar nuevas citas hasta que confirmes o canceles la cita pendiente.",
                        "OK");

                    await Navigation.PopAsync();
                    return;
                }

                if (servicePicker.SelectedItem == null)
                {
                    await DisplayAlert("Error", "Por favor selecciona un servicio.", "OK");
                    return;
                }

                if (_horarioSeleccionado == null)
                {
                    await DisplayAlert("Error", "Por favor selecciona un horario.", "OK");
                    return;
                }

                var servicioSeleccionado = (Servicio)servicePicker.SelectedItem;

                var nuevaCita = new CitaCreacionDto
                {
                    IdCliente = cliente.IdCliente,
                    IdServicio = servicioSeleccionado.IdServicio,
                    IdEmpleado = _horarioSeleccionado.IdEmpleado,
                    FechaInicio = _horarioSeleccionado.FechaInicio,
                    FechaFin = _horarioSeleccionado.FechaFin,
                    Estado = "Pendiente",
                    Notas = string.IsNullOrWhiteSpace(notasEditor.Text) ? string.Empty : notasEditor.Text.Trim()
                };

                btnConfirmar.IsEnabled = false;
                btnConfirmar.Text = "Confirmando...";

                var citaCreada = await citasService.CreateCitaAsync(nuevaCita);

                if (citaCreada != null)
                {
                    await DisplayAlert("Éxito",
                        $"¡Cita agendada exitosamente!\n\n" +
                        $"Servicio: {servicioSeleccionado.Descripcion}\n" +
                        $"Fecha: {_horarioSeleccionado.FechaInicio:dd/MM/yyyy}\n" +
                        $"Hora: {_horarioSeleccionado.FechaInicio:HH:mm}\n" +
                        $"Empleado: {_horarioSeleccionado.NombreEmpleado}",
                        "OK");

                    LimpiarFormulario();

                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo crear la cita. Inténtalo de nuevo.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al procesar la cita: {ex.Message}", "OK");
            }
            finally
            {
                btnConfirmar.IsEnabled = true;
                btnConfirmar.Text = "Confirmar Cita";
            }
        }

        private void LimpiarFormulario()
        {
            servicePicker.SelectedIndex = -1;
            datePicker.Date = DateTime.Today.AddDays(1);
            gridHorarios.Children.Clear();
            frameCitaSeleccionada.IsVisible = false;
            notasEditor.Text = string.Empty;
            lblDuracion.Text = "--";
            lblPrecio.Text = "--";
            btnConfirmar.IsEnabled = false;
            _horarioSeleccionado = null;
        }
    }
}