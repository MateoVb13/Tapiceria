using Newtonsoft.Json;
using System.Text;
using Tapiceria.Models;
using Tapiceria.Config;
using Microsoft.Maui.Storage; // Necesario para usar Preferences
using Microsoft.Maui.Controls;
using System.Diagnostics; // Para Debug.WriteLine

namespace Tapiceria.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        // Método para manejar el inicio de sesión en LoginPage.xaml.cs

        // Método para manejar el inicio de sesión en LoginPage.xaml.cs

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(entryCorreo.Text) || string.IsNullOrEmpty(entryContrasena.Text))
            {
                await DisplayAlert("Error", "Por favor ingresa tu correo y contraseña", "OK");
                return;
            }

            try
            {
                // Mostrar indicador de carga si lo tienes
                // activityIndicator.IsRunning = true;

                // Crear la solicitud de login
                var loginRequest = new LoginRequest
                {
                    Correo = entryCorreo.Text,
                    Contrasena = entryContrasena.Text
                };

                // Llamar al servicio de autenticación (necesitarías implementar esto)
                var client = new HttpClient();
                var content = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(loginRequest),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync($"{ApiConfig.BaseUrl}api/usuarios/login", content);

                if (response.IsSuccessStatusCode)
                {
                    // Deserializar la respuesta
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginResponse>(responseContent);

                    if (loginResponse != null && loginResponse.Usuario != null)
                    {
                        // Guardar datos del usuario en las preferencias
                        Preferences.Set("UserId", loginResponse.Usuario.IdUsuario);
                        Preferences.Set("Username", loginResponse.Usuario.NombreUsuario);

                        // Obtener el cliente asociado al usuario
                        var clientResponse = await client.GetAsync($"{ApiConfig.BaseUrl}api/clientes/PorUsuario/{loginResponse.Usuario.IdUsuario}");

                        if (clientResponse.IsSuccessStatusCode)
                        {
                            var clienteContent = await clientResponse.Content.ReadAsStringAsync();
                            var cliente = Newtonsoft.Json.JsonConvert.DeserializeObject<Cliente>(clienteContent);

                            if (cliente != null)
                            {
                                // Guardar el ID del cliente para usarlo en las citas
                                Preferences.Set("ClienteId", cliente.IdCliente);
                            }
                        }

                        // Navegar a la página de inicio
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


    }
}