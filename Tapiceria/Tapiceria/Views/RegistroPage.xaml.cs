// Archivo: Tapiceria/Views/RegistroPage.xaml.cs
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tapiceria.Models; // Asegúrate de que Usuarios esté aquí
using Tapiceria.Config; // Asegúrate de que ApiConfig esté aquí
using Microsoft.Maui.Controls;

namespace Tapiceria.Views
{
    public partial class RegistroPage : ContentPage
    {
        public RegistroPage()
        {
            InitializeComponent();
        }

        private async void OnRegistrarClicked(object sender, EventArgs e)
        {
            // Validación básica (puedes añadir más validaciones como formato de correo, longitud de contraseña)
            if (string.IsNullOrWhiteSpace(entryNombreUsuario.Text) || string.IsNullOrWhiteSpace(entryCorreo.Text) || string.IsNullOrWhiteSpace(entryContrasena.Text))
            {
                await DisplayAlert("Error", "Por favor, completa todos los campos.", "OK");
                return;
            }

            // Creamos un objeto Usuarios según lo que espera la API para el registro
            var usuario = new Usuarios
            {
                NombreUsuario = entryNombreUsuario.Text,
                Correo = entryCorreo.Text,
                Contrasena = entryContrasena.Text // La API se encargará del hashing
            };

            try
            {
                using var client = new HttpClient();
                var url = $"{ApiConfig.BaseUrl}api/Usuarios/registro"; // Endpoint de registro

                var json = JsonConvert.SerializeObject(usuario); // Serializamos el objeto Usuarios
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    // El registro fue exitoso (código 2xx)
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Opcional: Deserializar la respuesta si la API devuelve datos del usuario registrado
                    // var usuarioRegistrado = JsonConvert.DeserializeObject<Usuarios>(responseContent); // Asumiendo que devuelve el objeto Usuarios

                    await DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");

                    Application.Current.MainPage = new NavigationPage(new InicioPage()); // O new NavigationPage(new LoginPage());
                }
                else
                {
                    // El registro falló (ej: 400 Bad Request, 409 Conflict si el correo ya existe, 500 Internal Server Error)
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Error en el registro: {response.StatusCode} - {error}", "OK");
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Error a nivel de red
                await DisplayAlert("Error de Conexión", $"No se pudo conectar al servidor. Verifica tu conexión a Internet o la URL de la API. Detalles: {httpEx.Message}", "OK");
            }
            catch (JsonException jsonEx)
            {
                // Error al serializar el request o deserializar la respuesta
                await DisplayAlert("Error de Datos", $"Error al procesar datos para el registro. Detalles: {jsonEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                // Otros errores inesperados
                await DisplayAlert("Error Inesperado", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }
    }
}