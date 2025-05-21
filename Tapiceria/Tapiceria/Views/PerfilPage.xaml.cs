using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Tapiceria.Models;
using Tapiceria.Services;
using System.Diagnostics;

namespace Tapiceria.Views
{
    public partial class PerfilPage : ContentPage
    {
        private PerfilService _perfilService;
        private Cliente _clienteActual;
        private UserDto _usuarioActual;

        public PerfilPage()
        {
            InitializeComponent();
            _perfilService = new PerfilService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarDatosPerfil();
            await CargarHistorialCitas();
        }

        private async Task CargarDatosPerfil()
        {
            try
            {
                // Intentar recuperar el usuario de múltiples formas
                string userJson = Preferences.Get("usuario_actual", string.Empty);
                int userId = Preferences.Get("UserId", 0);
                string username = Preferences.Get("Username", string.Empty);

                Debug.WriteLine($"JSON recuperado: {userJson}");
                Debug.WriteLine($"UserId recuperado: {userId}");
                Debug.WriteLine($"Username recuperado: {username}");

                if (!string.IsNullOrEmpty(userJson))
                {
                    try
                    {
                        // Intentar deserializar el JSON
                        _usuarioActual = JsonConvert.DeserializeObject<UserDto>(userJson);
                        Debug.WriteLine("Usuario deserializado correctamente");
                    }
                    catch (Exception jsonEx)
                    {
                        Debug.WriteLine($"Error al deserializar JSON: {jsonEx.Message}");
                        // Si falla, crear objeto con datos individuales
                        _usuarioActual = new UserDto
                        {
                            IdUsuario = userId,
                            NombreUsuario = username,
                            Correo = "" // No tenemos el correo guardado individualmente
                        };
                    }
                }
                else if (userId > 0)
                {
                    // Si no hay JSON pero sí ID, crear objeto con datos individuales
                    _usuarioActual = new UserDto
                    {
                        IdUsuario = userId,
                        NombreUsuario = username,
                        Correo = "" // No tenemos el correo guardado individualmente
                    };
                }
                else
                {
                    await DisplayAlert("Error", "No hay información del usuario. Por favor, inicia sesión nuevamente.", "Aceptar");
                    await Navigation.PopAsync();
                    return;
                }

                // Establecer los datos del usuario en la UI
                lblNombreUsuario.Text = _usuarioActual.NombreUsuario;
                lblCorreo.Text = _usuarioActual.Correo;

                // Obtener datos del cliente asociado al usuario
                int clienteId = Preferences.Get("ClienteId", 0);
                if (clienteId > 0)
                {
                    // Si tenemos ClienteId, intentar obtener directo
                    _clienteActual = await _perfilService.GetClienteByIdAsync(clienteId);
                }

                if (_clienteActual == null && _usuarioActual.IdUsuario > 0)
                {
                    // Si no tenemos cliente o falló, intentar por IdUsuario
                    _clienteActual = await _perfilService.GetClienteByUsuarioIdAsync(_usuarioActual.IdUsuario);
                }

                if (_clienteActual != null)
                {
                    // Actualizar los campos con los datos del cliente
                    entryContacto.Text = _clienteActual.Contacto;
                    entryDireccion.Text = _clienteActual.Direccion;

                    // Guardar el ID del cliente en preferencias por si no estaba
                    Preferences.Set("ClienteId", _clienteActual.IdCliente);
                }
                else
                {
                    Debug.WriteLine("No se pudo obtener información del cliente");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en CargarDatosPerfil: {ex.Message}");
                await DisplayAlert("Error", $"No se pudieron cargar los datos: {ex.Message}", "Aceptar");
            }
        }

        private async Task CargarHistorialCitas()
        {
            if (_clienteActual == null) return;

            try
            {
                activityIndicator.IsRunning = true;

                var citasCompletadas = await _perfilService.GetCitasCompletadasAsync(_clienteActual.IdCliente);

                if (citasCompletadas == null || citasCompletadas.Count == 0)
                {
                    lblSinHistorial.IsVisible = true;
                    citasCompletadasCollection.IsVisible = false;
                }
                else
                {
                    lblSinHistorial.IsVisible = false;
                    citasCompletadasCollection.IsVisible = true;
                    citasCompletadasCollection.ItemsSource = citasCompletadas;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en CargarHistorialCitas: {ex.Message}");
                lblSinHistorial.IsVisible = true;
                citasCompletadasCollection.IsVisible = false;
            }
            finally
            {
                activityIndicator.IsRunning = false;
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            if (_clienteActual == null)
            {
                await DisplayAlert("Error", "No se encontró información del cliente.", "Aceptar");
                return;
            }

            if (string.IsNullOrWhiteSpace(entryContacto.Text) || string.IsNullOrWhiteSpace(entryDireccion.Text))
            {
                await DisplayAlert("Datos incompletos", "Por favor ingresa un número de contacto y dirección válidos.", "Aceptar");
                return;
            }

            try
            {
                // Mostrar indicador de carga
                if (activityIndicator != null)
                    activityIndicator.IsRunning = true;

                // Guardar valores originales para depuración
                var contactoOriginal = _clienteActual.Contacto;
                var direccionOriginal = _clienteActual.Direccion;

                // Actualizar propiedades del cliente
                _clienteActual.Contacto = entryContacto.Text.Trim();
                _clienteActual.Direccion = entryDireccion.Text.Trim();

                System.Diagnostics.Debug.WriteLine($"Iniciando actualización:");
                System.Diagnostics.Debug.WriteLine($"   - ID Cliente: {_clienteActual.IdCliente}");
                System.Diagnostics.Debug.WriteLine($"   - Contacto: '{contactoOriginal}' => '{_clienteActual.Contacto}'");
                System.Diagnostics.Debug.WriteLine($"   - Dirección: '{direccionOriginal}' => '{_clienteActual.Direccion}'");

                // Llamar al servicio para actualizar
                bool exito = await _perfilService.UpdateClienteAsync(_clienteActual);

                System.Diagnostics.Debug.WriteLine($"Resultado de actualización: {(exito ? "ÉXITO" : "FALLO")}");

                if (exito)
                {
                    // Guardar el estado de completado en preferencias
                    Preferences.Set("datos_cliente_completos", true);

                    System.Diagnostics.Debug.WriteLine("Mostrando mensaje de éxito");
                    await DisplayAlert("Éxito", "Tu información ha sido actualizada correctamente.", "Aceptar");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Mostrando mensaje de error");
                    await DisplayAlert("Error", "No se pudieron guardar los cambios. Verifica tu conexión e intenta nuevamente.", "Aceptar");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excepción no capturada en OnGuardarClicked: {ex.Message}");
                await DisplayAlert("Error", $"Ocurrió un error inesperado: {ex.Message}", "Aceptar");
            }
            finally
            {
                // Ocultar indicador de carga
                if (activityIndicator != null)
                    activityIndicator.IsRunning = false;
            }
        }
    }
}