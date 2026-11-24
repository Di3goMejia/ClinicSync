
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class AgregarTratamientoWindow : Window
    {
        /// <summary>
        /// The validador
        /// </summary>
        private readonly ValidadorTratamiento _validador = new();
        /// <summary>
        /// The controles
        /// </summary>
        private readonly Dictionary<string, Control> _controles;
        /// <summary>
        /// The tratamientos service
        /// </summary>
        private readonly TratamientosApiService? _tratamientosService;
        /// <summary>
        /// The guardando
        /// </summary>
        private bool _guardando;

        /// <summary>
        /// Gets a value indicating whether [error al registrar].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [error al registrar]; otherwise, <c>false</c>.
        /// </value>
        public bool ErrorAlRegistrar { get; private set; }

        /// <summary>
        /// Gets the mensaje error.
        /// </summary>
        /// <value>
        /// The mensaje error.
        /// </value>
        public string MensajeError { get; private set; } =
            "No se pudo registrar el tratamiento.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AgregarTratamientoWindow"/> class.
        /// </summary>
        /// <param name="tratamientosService">The tratamientos service.</param>
        public AgregarTratamientoWindow(
            TratamientosApiService tratamientosService)
        {
            InitializeComponent();
            _tratamientosService = tratamientosService;
            _controles = new Dictionary<string, Control>
            {
                [nameof(EntradaTratamiento.Nombre)] = TxtNombre,
                [nameof(EntradaTratamiento.TextoPrecio)] = TxtPrecio,
                [nameof(EntradaTratamiento.Descripcion)] = TxtDescripcion
            };
        }

        /// <summary>
        /// Mostrars the error.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        private void MostrarError(string mensaje)
        {
            TxtErrorGeneral.Text = mensaje;
            TxtErrorGeneral.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Ocultars the error.
        /// </summary>
        private void OcultarError()
        {
            TxtErrorGeneral.Text = string.Empty;
            TxtErrorGeneral.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the TextChanged event of the TxtNombre control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TxtNombre_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            OcultarError();
        }

        /// <summary>
        /// Handles the TextChanged event of the TxtPrecio control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TxtPrecio_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            OcultarError();
        }

        /// <summary>
        /// Handles the TextChanged event of the TxtDescripcion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TxtDescripcion_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            OcultarError();
        }

        /// <summary>
        /// Handles the Click event of the Cerrar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            bool tieneDatos =
                !string.IsNullOrWhiteSpace(TxtNombre.Text) ||
                !string.IsNullOrWhiteSpace(TxtPrecio.Text) ||
                !string.IsNullOrWhiteSpace(TxtDescripcion.Text);

            if (tieneDatos)
            {
                ConfirmarDescarteWindow advertencia =
                    new ConfirmarDescarteWindow(
                        "¿Seguro que desea descartar este registro?")
                    {
                        Owner = this
                    };

                bool descartar = advertencia.ShowDialog() == true;

                if (!descartar)
                    return;
            }

            DialogResult = false;
        }

        /// <summary>
        /// Validars the formulario.
        /// </summary>
        /// <param name="precio">The precio.</param>
        /// <returns></returns>
        public bool ValidarFormulario(out decimal precio)
        {
            var entrada = new EntradaTratamiento(
                Nombre: TxtNombre.Text.Trim(),
                TextoPrecio: TxtPrecio.Text.Trim(),
                Descripcion: TxtDescripcion.Text.Trim()
            );

            var resultado = _validador.Validar(entrada);

            if (!HelperValidacion.AplicarResultado(resultado, _controles, MostrarError, OcultarError))
            {
                precio = 0m;
                return false;
            }

            precio = resultado.PrecioNormalizado;
            return true;
        }

        /// <summary>
        /// Handles the Click event of the Agregar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Agregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_guardando)
                return;

            OcultarError();

            if (_tratamientosService is null)
            {
                MostrarError("La conexión con la API no está configurada.");
                return;
            }

            if (!ValidarFormulario(out decimal precio))
                return;

            _guardando = true;
            btnAgregar.IsEnabled = false;

            try
            {
                await _tratamientosService.RegistrarAsync(
                    TxtNombre.Text.Trim(),
                    precio,
                    string.IsNullOrWhiteSpace(TxtDescripcion.Text)
                        ? null
                        : TxtDescripcion.Text.Trim());

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorAlRegistrar = false;

                if (ex.Message.Contains(
                    "Ya existe un tratamiento con ese nombre",
                    StringComparison.OrdinalIgnoreCase))
                {
                    MostrarError("El tratamiento ya existe.");
                }
                else
                {
                    MostrarError(
                        "No se pudo registrar el tratamiento. Verifica la conexión e inténtalo nuevamente.");
                }
            }
            finally
            {
                _guardando = false;
                btnAgregar.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Barra control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Barra_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
