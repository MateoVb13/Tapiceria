using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tapiceria.Models;
using Tapiceria.Config; // Agregamos el namespace donde está ApiConfig
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
            var usuario = new Usuarios
            {
                NombreUsuario = entryNombreUsuario.Text,
                Correo = entryCorreo.Text,
                Contrasena = entryContrasena.Text
            };

            try
            {
                using var client = new HttpClient();
                var url = $"{ApiConfig.BaseUrl}api/Usuarios/registro"; // Usamos la variable global

                var json = JsonConvert.SerializeObject(usuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");
                    // Redirigir o limpiar campos aquí si deseas
                    await Navigation.PushAsync(new InicioPage());
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Error en el registro: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Excepción", ex.Message, "OK");
            }
        }
    }
}
