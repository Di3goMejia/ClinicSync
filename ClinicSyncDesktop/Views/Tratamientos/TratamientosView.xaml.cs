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
        private ICollectionView? _vista;

        public TratamientosView()
        {
            InitializeComponent();
        }

        public TratamientosView(
            TratamientosApiService tratamientosService)
            : this()
        {
            _tratamientosService = tratamientosService;
            Loaded += TratamientosView_Loaded;
        }

        // Mostrar notificaciones.
        private void MostrarAviso(string mensaje, bool exitoso)
        {
            PanelAvisos.Children.Clear();

            AvisoNotificacion aviso = new AvisoNotificacion(
                mensaje,
                exitoso);

            PanelAvisos.Children.Add(aviso);
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
                MostrarAviso(
                    "La conexión con la API todavía no está configurada.",
                    false);

                return;
            }

            try
            {
                var listado =
                    await _tratamientosService.ListarAsync();

                var coleccion =
                    new ObservableCollection<Tratamiento>(listado);

                _vista =
                    CollectionViewSource.GetDefaultView(coleccion);

                _vista.Filter = FiltrarTratamiento;

                dgTratamientos.ItemsSource = _vista;
            }
            catch (Exception)
            {
                MostrarAviso(
                    "No fue posible cargar los tratamientos. Verifica el servidor y tus permisos.",
                    false);
            }
        }

        private async void BtnAgregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_tratamientosService is null)
            {
                MostrarAviso(
                    "La conexión con la API todavía no está configurada.",
                    false);

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

                MostrarAviso(
                    "Tratamiento registrado exitosamente.",
                    true);
            }
            else if (ventana.ErrorAlRegistrar)
            {
                MostrarAviso(
                    ventana.MensajeError,
                    false);
            }
        }

        public event EventHandler? VolverAlMenuSolicitado;

        private void BtnVolver_Click(
            object sender,
            RoutedEventArgs e)
        {
            VolverAlMenuSolicitado?.Invoke(
                this,
                EventArgs.Empty);
        }

        private bool FiltrarTratamiento(object obj)
        {
            if (obj is not Tratamiento tratamiento)
                return false;

            string texto = TxtBuscar.Text.Trim();

            if (string.IsNullOrEmpty(texto))
                return true;

            return tratamiento.Nombre.Contains(
                texto,
                StringComparison.OrdinalIgnoreCase);
        }

        private void TxtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            _vista?.Refresh();
        }

        private async void BtnOpciones_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.DataContext is not Tratamiento tratamiento)
            {
                return;
            }

            if (_tratamientosService is null)
            {
                MostrarAviso(
                    "La conexión con la API todavía no está configurada.",
                    false);

                return;
            }

            var ventana = new EditarTratamientoWindow(
                tratamiento,
                _tratamientosService)
            {
                Owner = Window.GetWindow(this)
            };

            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                await CargarTratamientosAsync();

                MostrarAviso(
                    "Tratamiento actualizado exitosamente.",
                    true);
            }
            else if (ventana.ErrorAlGuardar)
            {
                MostrarAviso(
                    ventana.MensajeError,
                    false);
            }
        }
    }
}