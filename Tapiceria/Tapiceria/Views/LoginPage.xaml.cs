using Newtonsoft.Json;
using System.Text;
using Tapiceria.Models;
using Tapiceria.Config;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace Tapiceria.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(entryCorreo.Text) || string.IsNullOrEmpty(entryContrasena.Text))
            {
                await DisplayAlert("Error", "Por favor ingresa tu correo y contraseña", "OK");
                return;
            }

            try
            {

                var loginRequest = new LoginRequest
                {
                    Correo = entryCorreo.Text,
                    Contrasena = entryContrasena.Text
                };

                var client = new HttpClient();
                var content = new StringContent(
                    JsonConvert.SerializeObject(loginRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync($"{ApiConfig.BaseUrl}api/usuarios/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                    if (loginResponse != null && loginResponse.Usuario != null)
                    {
                        var usuarioJson = JsonConvert.SerializeObject(loginResponse.Usuario);
                        Preferences.Set("usuario_actual", usuarioJson);

                        Preferences.Set("UserId", loginResponse.Usuario.IdUsuario);
                        Preferences.Set("Username", loginResponse.Usuario.NombreUsuario);

                        Debug.WriteLine($"Usuario guardado: {usuarioJson}");

                        var clientResponse = await client.GetAsync($"{ApiConfig.BaseUrl}api/clientes/PorUsuario/{loginResponse.Usuario.IdUsuario}");

                        if (clientResponse.IsSuccessStatusCode)
                        {
                            var clienteContent = await clientResponse.Content.ReadAsStringAsync();
                            var cliente = JsonConvert.DeserializeObject<Cliente>(clienteContent);

                            if (cliente != null)
                            {
                                Preferences.Set("ClienteId", cliente.IdCliente);

                                bool datosCompletos = !string.IsNullOrWhiteSpace(cliente.Contacto) &&
                                                     !string.IsNullOrWhiteSpace(cliente.Direccion);
                                Preferences.Set("datos_cliente_completos", datosCompletos);
                            }
                        }

                        await Navigation.PushAsync(new InicioPage());
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudieron obtener los datos del usuario", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Credenciales incorrectas", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
            finally
            {
                // Ocultar indicador de carga
                // activityIndicator.IsRunning = false;
            }
        }
        private async void OnNavigateToRegister(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistroPage());
        }


    }
}