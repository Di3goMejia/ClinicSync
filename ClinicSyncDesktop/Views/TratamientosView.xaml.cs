using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClinicSyncDesktop.Views
{
    public partial class TratamientosView : UserControl
    {
        private readonly TratamientosApiService? _tratamientosService;
        private ICollectionView _vista = null!;

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
                var listado = await _tratamientosService.ListarAsync();
                var coleccion = new ObservableCollection<Tratamiento>(listado);

                _vista = CollectionViewSource.GetDefaultView(coleccion);
                _vista.Filter = FiltrarTratamiento;

                dgTratamientos.ItemsSource = _vista;
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

        private async void BtnAgregar_Click(
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

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            VolverAlMenuSolicitado?.Invoke(this, EventArgs.Empty);
        }

        private bool FiltrarTratamiento(object obj)
        {
            if (obj is not Tratamiento tr) 
                return false;

            var texto = TxtBuscar.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(texto)) 
                return true;

            return tr.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase);
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            _vista?.Refresh();
        }

        private async void BtnOpciones_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.DataContext is Tratamiento tr)
            {
                var dialogo = new EditarTratamientoWindow(tr, _tratamientosService!)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialogo.ShowDialog() == true)
                {
                    await CargarTratamientosAsync();
                }
            }
        }
    }
}
