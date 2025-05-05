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

        private async void OnLoginClicked(object sender, EventArgs e)
        {

            var request = new LoginRequest // Usa el DTO de Request, no el modelo completo Usuarios
            {
                Correo = entryCorreo.Text,
                Contrasena = entryContrasena.Text
            };

            try
            {
                using var client = new HttpClient();
                var url = $"{ApiConfig.BaseUrl}api/Usuarios/login"; // Endpoint de login en tu API

                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    // Tu API de login retorna un objeto con Message y User (que es un UserDto)
                    var loginResult = JsonConvert.DeserializeObject<LoginResponse>(jsonResponse);

                    if (loginResult != null && loginResult.Usuario != null)
                    {
                        // *** GUARDAR EL ID DEL USUARIO LOGUEADO EN PREFERENCES ***
                        Preferences.Set("LoggedInUserId", loginResult.Usuario.IdUsuario);
                        Debug.WriteLine($"Usuario logueado: {loginResult.Usuario.IdUsuario}"); // Para depuración

                        await DisplayAlert("Éxito", loginResult.Mensaje, "OK");

                        // Navegar a la página de inicio (InicioPage)
                        await Navigation.PushAsync(new InicioPage());

                        // Opcional: Limpiar campos de login después de navegar
                        entryCorreo.Text = string.Empty;
                        entryContrasena.Text = string.Empty;
                    }
                    else
                    {
                        // Esto podría pasar si la API retorna 200 OK pero con un cuerpo inesperado
                        await DisplayAlert("Error", "Respuesta de API inesperada.", "OK");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Manejar credenciales inválidas (si tu API retorna 401 Unauthorized)
                    await DisplayAlert("Error de Login", "Correo o contraseña inválidos.", "OK");
                }
                else
                {
                    // Manejar otros errores de la API
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error en la API", $"Error al iniciar sesión: {response.StatusCode} - {error}", "OK");
                }
            }
            catch (HttpRequestException httpEx)
            {
                await DisplayAlert("Error de Conexión", $"No se pudo conectar al servidor. Verifica la URL de la API. Detalles: {httpEx.Message}", "OK");
            }
            catch (JsonException jsonEx)
            {
                await DisplayAlert("Error de Datos", $"Error al procesar la respuesta del servidor. Detalles: {jsonEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error Inesperado", $"Ocurrió un error durante el login: {ex.Message}", "OK");
            }

        }


    }
}