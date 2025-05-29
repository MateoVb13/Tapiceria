using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tapiceria.Models;
using Tapiceria.Services;

namespace Tapiceria.Views
{
    public partial class MisCitasPage : ContentPage, INotifyPropertyChanged
    {
        private readonly CitasService _citasService;
        private int _clienteId;
        private bool _isRefreshing;
        private ObservableCollection<CitaListItemDto> _citas;

        // Propiedad para controlar el estado de recarga
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    OnPropertyChanged();
                }
            }
        }

        // Colección observable de citas
        public ObservableCollection<CitaListItemDto> Citas
        {
            get => _citas;
            set
            {
                if (_citas != value)
                {
                    _citas = value;
                    OnPropertyChanged();
                }
            }
        }

        // Comando para refrescar la lista
        public ICommand RefreshCommand { get; }

        // Constructor
        public MisCitasPage()
        {
            InitializeComponent();

            // Inicializar servicio y colección
            _citasService = new CitasService();
            _citas = new ObservableCollection<CitaListItemDto>();

            // Configurar comando de refresco
            RefreshCommand = new Command(async () =>
            {
                IsRefreshing = true;
                await LoadCitas();
                IsRefreshing = false;
            });

            // Enlazar la colección y contexto
            citasCollectionView.ItemsSource = _citas;
            this.BindingContext = this;

            // Obtener ID de cliente desde preferencias
            _clienteId = Preferences.Get("ClienteId", 0);

            // Si no hay ID de cliente, intentar obtenerlo del ID de usuario
            if (_clienteId <= 0)
            {
                int userId = Preferences.Get("UserId", 0);
                if (userId > 0)
                {
                    // Necesitaríamos implementar un método para obtener el cliente por ID de usuario
                    // Por ahora, solo mostramos una alerta
                    DisplayAlert("Información", "Necesitas iniciar sesión para ver tus citas", "OK");
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Cargar las citas cada vez que la página se muestra
            await LoadCitas();
        }

        private async Task LoadCitas()
        {
            if (_clienteId <= 0)
            {
                await DisplayAlert("Error", "No se pudo identificar al cliente. Por favor inicia sesión nuevamente.", "OK");
                return;
            }

            try
            {
                // Mostrar indicador de carga
                activityIndicator.IsRunning = true;

                // Limpiar lista actual
                _citas.Clear();

                // Cargar citas desde la API
                var citasCliente = await _citasService.GetCitasByClienteIdAsync(_clienteId);

                if (citasCliente != null)
                {
                    // Agregar citas a la colección observable
                    foreach (var cita in citasCliente.OrderBy(c => c.FechaInicio))
                    {
                        _citas.Add(cita);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar las citas: {ex.Message}", "OK");
            }
            finally
            {
                // Ocultar indicador de carga
                activityIndicator.IsRunning = false;
            }
        }


        private async void OnNuevaCitaClicked(object sender, EventArgs e)
        {
            try
            {
                // Obtener el cliente actual
                var cliente = await PerfilService.GetClienteActual();

                if (cliente == null)
                {
                    await DisplayAlert("Error", "No se pudo obtener la información del cliente.", "OK");
                    return;
                }

                // Verificar si tiene citas pendientes
                var citasService = new Services.CitasService();
                bool tieneCitasPendientes = await citasService.TieneCitasPendientesAsync(cliente.IdCliente);

                if (tieneCitasPendientes)
                {
                    await DisplayAlert("Restricción",
                        "Tienes una cita pendiente. No puedes agendar nuevas citas hasta que confirmes o canceles la cita pendiente.",
                        "Entendido");
                    return;
                }

                // Si no tiene citas pendientes, permitir agendar
                await Navigation.PushAsync(new AgendarCitaPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al verificar disponibilidad: {ex.Message}", "OK");
            }
        }

        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}