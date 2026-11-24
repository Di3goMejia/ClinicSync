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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class TratamientosView : UserControl
    {
        /// <summary>
        /// The tratamientos service
        /// </summary>
        private readonly TratamientosApiService? _tratamientosService;
        /// <summary>
        /// The vista
        /// </summary>
        private ICollectionView? _vista;

        /// <summary>
        /// Initializes a new instance of the <see cref="TratamientosView"/> class.
        /// </summary>
        public TratamientosView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TratamientosView"/> class.
        /// </summary>
        /// <param name="tratamientosService">The tratamientos service.</param>
        public TratamientosView(
            TratamientosApiService tratamientosService)
            : this()
        {
            _tratamientosService = tratamientosService;
            Loaded += TratamientosView_Loaded;
        }

        // Mostrar notificaciones.
        /// <summary>
        /// Mostrars the aviso.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="exitoso">if set to <c>true</c> [exitoso].</param>
        private void MostrarAviso(string mensaje, bool exitoso)
        {
            PanelAvisos.Children.Clear();

            AvisoNotificacion aviso = new AvisoNotificacion(
                mensaje,
                exitoso);

            PanelAvisos.Children.Add(aviso);
        }

        /// <summary>
        /// Handles the Loaded event of the TratamientosView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void TratamientosView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarTratamientosAsync();
        }

        /// <summary>
        /// Cargars the tratamientos asynchronous.
        /// </summary>
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

        /// <summary>
        /// Handles the Click event of the BtnAgregar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Occurs when [volver al menu solicitado].
        /// </summary>
        public event EventHandler? VolverAlMenuSolicitado;

        /// <summary>
        /// Handles the Click event of the BtnVolver control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnVolver_Click(
            object sender,
            RoutedEventArgs e)
        {
            VolverAlMenuSolicitado?.Invoke(
                this,
                EventArgs.Empty);
        }

        /// <summary>
        /// Filtrars the tratamiento.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handles the TextChanged event of the TxtBuscar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TxtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            _vista?.Refresh();
        }

        /// <summary>
        /// Handles the Click event of the BtnOpciones control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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