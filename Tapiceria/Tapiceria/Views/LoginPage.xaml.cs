using Newtonsoft.Json;
using System.Text;
using Tapiceria.Models;
using Tapiceria.Config;

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
            var usuario = new Usuarios
            {
                NombreUsuario = "string",
                Correo = entryCorreo.Text,
                Contrasena = entryContrasena.Text
            };

            try
            {
                using var client = new HttpClient();
                var url = $"{ApiConfig.BaseUrl}api/Usuarios/login";

                var json = JsonConvert.SerializeObject(usuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Sesión iniciada correctamente", "OK");
                    // Redirigir o limpiar campos aquí si deseas
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Error al iniciar sesión: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
            }
        }
    }
}
