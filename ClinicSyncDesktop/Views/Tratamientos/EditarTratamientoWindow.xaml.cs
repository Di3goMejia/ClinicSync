
using ClinicSyncDesktop.Models;
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
    public partial class EditarTratamientoWindow : Window
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
        private readonly TratamientosApiService _tratamientosService;
        /// <summary>
        /// The tratamiento
        /// </summary>
        private readonly Tratamiento _tratamiento;

        /// <summary>
        /// The guardando
        /// </summary>
        private bool _guardando;

        // Estado guardado del tratamiento al abrir la ventana.
        /// <summary>
        /// The estado inicial activo
        /// </summary>
        private bool _estadoInicialActivo;

        /// <summary>
        /// Gets a value indicating whether [error al guardar].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [error al guardar]; otherwise, <c>false</c>.
        /// </value>
        public bool ErrorAlGuardar { get; private set; }

        /// <summary>
        /// Gets the mensaje error.
        /// </summary>
        /// <value>
        /// The mensaje error.
        /// </value>
        public string MensajeError { get; private set; } =
            "No se pudo actualizar el tratamiento.";

        /// <summary>
        /// Initializes a new instance of the <see cref="EditarTratamientoWindow"/> class.
        /// </summary>
        /// <param name="tr">The tr.</param>
        /// <param name="svc">The SVC.</param>
        public EditarTratamientoWindow(
            Tratamiento tr,
            TratamientosApiService svc)
        {
            InitializeComponent();

            _tratamiento = tr;
            _tratamientosService = svc;

            _controles = new Dictionary<string, Control>
            {
                [nameof(EntradaTratamiento.Nombre)] = TxtNombre,
                [nameof(EntradaTratamiento.TextoPrecio)] = TxtPrecio,
                [nameof(EntradaTratamiento.Descripcion)] = TxtDescripcion
            };

            // Capturar el estado original antes de permitir cambios.
            _estadoInicialActivo = _tratamiento.Activo;

            TxtNombre.Text = _tratamiento.Nombre;

            TxtPrecio.Text = _tratamiento.Precio.ToString(
                "0.00",
                CultureInfo.InvariantCulture);

            TxtDescripcion.Text =
                _tratamiento.Descripcion ?? "";

            ChkActivo.IsChecked = _tratamiento.Activo;

            ActualizarTextoEstado();
            ActualizarControlesEstado();
        }

        // ACTUALIZAR TEXTO DEL ESTADO
        /// <summary>
        /// Actualizars the texto estado.
        /// </summary>
        private void ActualizarTextoEstado()
        {
            TxtEstado.Text = ChkActivo.IsChecked == true
                ? "Habilitado"
                : "Deshabilitado";
        }

        // ACTUALIZAR CAMPOS SEGÚN EL ESTADO GUARDADO
        /// <summary>
        /// Actualizars the controles estado.
        /// </summary>
        private void ActualizarControlesEstado()
        {
            // El estado inicial determina si se pueden editar los datos.
            // Cambiar el interruptor no modifica este valor.
            bool permitirEdicion = _estadoInicialActivo;

            TxtNombre.IsEnabled = permitirEdicion;
            TxtPrecio.IsEnabled = permitirEdicion;
            TxtDescripcion.IsEnabled = permitirEdicion;

            // El interruptor siempre debe poder utilizarse.
            ChkActivo.IsEnabled = true;

            // Evitar varios guardados simultáneos.
            BtnGuardar.IsEnabled = !_guardando;

            // Mostrar el aviso si el tratamiento ya estaba deshabilitado
            // al abrir la ventana.
            TxtAvisoEstado.Visibility = permitirEdicion
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // MOSTRAR ERROR
        /// <summary>
        /// Mostrars the error.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        private void MostrarError(string mensaje)
        {
            TxtErrorGeneral.Text = mensaje;
            TxtErrorGeneral.Visibility = Visibility.Visible;
        }

        // OCULTAR ERROR
        /// <summary>
        /// Ocultars the error.
        /// </summary>
        private void OcultarError()
        {
            TxtErrorGeneral.Text = string.Empty;
            TxtErrorGeneral.Visibility = Visibility.Collapsed;
        }

        // CAMBIOS EN EL NOMBRE
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

        // CAMBIOS EN EL PRECIO
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

        // CAMBIOS EN LA DESCRIPCIÓN
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

        // CERRAR VENTANA
        /// <summary>
        /// Handles the Click event of the Cerrar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            string nombre = TxtNombre.Text.Trim();
            string precio = TxtPrecio.Text.Trim();
            string descripcion = TxtDescripcion.Text.Trim();
            bool activo = ChkActivo.IsChecked == true;

            bool tieneCambios =
                nombre != _tratamiento.Nombre ||
                precio != _tratamiento.Precio.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture) ||
                descripcion != (_tratamiento.Descripcion ?? "") ||
                activo != _tratamiento.Activo;

            if (tieneCambios)
            {
                ConfirmarDescarteWindow advertencia =
                    new ConfirmarDescarteWindow(
                        "¿Seguro que desea descartar esta edición?")
                    {
                        Owner = this
                    };

                bool descartar = advertencia.ShowDialog() == true;

                if (!descartar)
                    return;
            }

            DialogResult = false;
        }

        // VALIDAR FORMULARIO
        /// <summary>
        /// Validars the formulario.
        /// </summary>
        /// <param name="precio">The precio.</param>
        /// <returns></returns>
        private bool ValidarFormulario(out decimal precio)
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

        // GUARDAR CAMBIOS
        /// <summary>
        /// Handles the Click event of the BtnGuardar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void BtnGuardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_guardando)
                return;

            OcultarError();

            bool activo = ChkActivo.IsChecked == true;

            // Por defecto, conservar los datos originales.
            string nombre = _tratamiento.Nombre;
            string descripcion = _tratamiento.Descripcion ?? "";
            decimal precio = _tratamiento.Precio;

            // Solo permitir modificar datos si el tratamiento estaba
            // habilitado al abrir la ventana.
            if (_estadoInicialActivo)
            {
                if (!ValidarFormulario(out precio))
                    return;

                nombre = TxtNombre.Text.Trim();
                descripcion = TxtDescripcion.Text.Trim();
            }

            bool cambioDatos =
                nombre != _tratamiento.Nombre ||
                precio != _tratamiento.Precio ||
                descripcion != (_tratamiento.Descripcion ?? "");

            bool cambioEstado =
                activo != _tratamiento.Activo;

            if (!cambioDatos && !cambioEstado)
            {
                DialogResult = false;
                return;
            }

            _guardando = true;
            ActualizarControlesEstado();

            try
            {
                // Actualizar datos solo cuando corresponda.
                if (cambioDatos)
                {
                    await _tratamientosService.ModificarAsync(
                        _tratamiento.IdTratamiento,
                        nombre,
                        precio,
                        descripcion);
                }

                // Guardar la activación o desactivación.
                if (cambioEstado)
                {
                    await _tratamientosService.CambiarEstadoAsync(
                        _tratamiento.IdTratamiento,
                        activo);
                }

                // Actualizar el objeto local después de guardar.
                _tratamiento.Nombre = nombre;
                _tratamiento.Descripcion = descripcion;
                _tratamiento.Precio = precio;
                _tratamiento.Activo = activo;

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorAlGuardar = true;

                if (ex.Message.Contains(
                    "Ya existe un tratamiento con ese nombre",
                    StringComparison.OrdinalIgnoreCase))
                {
                    MensajeError =
                        "El tratamiento ya existe.";

                    MostrarError(MensajeError);
                }
                else
                {
                    MensajeError =
                        "No se pudo actualizar el tratamiento. " +
                        "Verifica la conexión e inténtalo nuevamente.";

                    MostrarError(MensajeError);
                }
            }
            finally
            {
                _guardando = false;
                ActualizarControlesEstado();
            }
        }

        // CAMBIAR EL INTERRUPTOR NO BLOQUEA LOS CAMPOS INMEDIATAMENTE
        /// <summary>
        /// Handles the Checked event of the ChkActivo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ChkActivo_Checked(
            object sender,
            RoutedEventArgs e)
        {
            ActualizarTextoEstado();
            OcultarError();
        }

        /// <summary>
        /// Handles the Unchecked event of the ChkActivo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ChkActivo_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            ActualizarTextoEstado();
            OcultarError();
        }

        // MOVER VENTANA
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
