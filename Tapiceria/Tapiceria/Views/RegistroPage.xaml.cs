using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tapiceria.Models;
using Tapiceria.Config;
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

            if (string.IsNullOrWhiteSpace(entryNombreUsuario.Text) || string.IsNullOrWhiteSpace(entryCorreo.Text) || string.IsNullOrWhiteSpace(entryContrasena.Text))
            {
                await DisplayAlert("Error", "Por favor, completa todos los campos.", "OK");
                return;
            }
            if (entryConfirmarContrasena.Text != entryContrasena.Text)
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
                return;
            }


            var usuario = new Usuarios
            {
                NombreUsuario = entryNombreUsuario.Text,
                Correo = entryCorreo.Text,
                Contrasena = entryContrasena.Text
            };

            try
            {
                using var client = new HttpClient();
                var url = $"{ApiConfig.BaseUrl}api/Usuarios/registro";

                var json = JsonConvert.SerializeObject(usuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {

                    var responseContent = await response.Content.ReadAsStringAsync();


                    await DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");

                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
                else
                {

                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Error en el registro: {response.StatusCode} - {error}", "OK");
                }
            }
            catch (HttpRequestException httpEx)
            {

                await DisplayAlert("Error de Conexión", $"No se pudo conectar al servidor. Verifica tu conexión a Internet o la URL de la API. Detalles: {httpEx.Message}", "OK");
            }
            catch (JsonException jsonEx)
            {

                await DisplayAlert("Error de Datos", $"Error al procesar datos para el registro. Detalles: {jsonEx.Message}", "OK");
            }
            catch (Exception ex)
            {

                await DisplayAlert("Error Inesperado", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }


        private async void OnNavigateToLogin(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }
    }
}