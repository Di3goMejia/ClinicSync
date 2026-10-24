using ClinicSyncDesktop.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    public partial class TratamientosView : UserControl
    {
        private readonly TratamientosApiService? _tratamientosService;

        public TratamientosView()
        {
            InitializeComponent();
        }

        public TratamientosView(TratamientosApiService tratamientosService)
            : this()
        {
            _tratamientosService = tratamientosService;
            Loaded += TratamientosView_Loaded;
        }

        private async void TratamientosView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarTratamientosAsync();
        }

        private async Task CargarTratamientosAsync()
        {
            if (_tratamientosService is null)
            {
                MessageBox.Show(
                    "La conexión con la API todavía no está configurada.",
                    "Tratamientos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            try
            {
                dgTratamientos.ItemsSource =
                    await _tratamientosService.ListarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "No fue posible cargar los tratamientos. Verifica que el servidor esté iniciado y que la sesión tenga autorización.",
                    "Error al cargar tratamientos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private async void btnAgregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_tratamientosService is null)
            {
                MessageBox.Show(
                    "La conexión con la API todavía no está configurada.",
                    "Tratamientos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var ventana = new AgregarTratamientoWindow(
                _tratamientosService)
            {
                Owner = Window.GetWindow(this)
            };

            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                await CargarTratamientosAsync();
            }
        }

        public event EventHandler? VolverAlMenuSolicitado;

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            VolverAlMenuSolicitado?.Invoke(this, EventArgs.Empty);
        }

    }
}
