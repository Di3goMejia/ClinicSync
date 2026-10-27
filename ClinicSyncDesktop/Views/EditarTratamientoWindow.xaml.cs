
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSyncDesktop.Views
{
    public partial class EditarTratamientoWindow : Window
    {
        private readonly TratamientosApiService _tratamientosService;
        private readonly Tratamiento _tratamiento;

        private bool _guardando;

        // Estado guardado del tratamiento al abrir la ventana.
        private bool _estadoInicialActivo;

        public bool ErrorAlGuardar { get; private set; }

        public string MensajeError { get; private set; } =
            "No se pudo actualizar el tratamiento.";

        public EditarTratamientoWindow(
            Tratamiento tr,
            TratamientosApiService svc)
        {
            InitializeComponent();

            _tratamiento = tr;
            _tratamientosService = svc;

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
        private void ActualizarTextoEstado()
        {
            TxtEstado.Text = ChkActivo.IsChecked == true
                ? "Habilitado"
                : "Deshabilitado";
        }

        // ACTUALIZAR CAMPOS SEGÚN EL ESTADO GUARDADO
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
        private void MostrarError(string mensaje)
        {
            TxtErrorGeneral.Text = mensaje;
            TxtErrorGeneral.Visibility = Visibility.Visible;
        }

        // OCULTAR ERROR
        private void OcultarError()
        {
            TxtErrorGeneral.Text = string.Empty;
            TxtErrorGeneral.Visibility = Visibility.Collapsed;
        }

        // CAMBIOS EN EL NOMBRE
        private void TxtNombre_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            OcultarError();
        }

        // CAMBIOS EN EL PRECIO
        private void TxtPrecio_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            OcultarError();
        }

        // CAMBIOS EN LA DESCRIPCIÓN
        private void TxtDescripcion_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            OcultarError();
        }

        // CERRAR VENTANA
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
                bool descartar = MensajeWindow.MostrarConfirmacion(
                    this,
                    "¿Seguro que desea descartar esta edición?");

                if (!descartar)
                    return;
            }

            DialogResult = false;
        }

        // VALIDAR FORMULARIO
        private bool ValidarFormulario(out decimal precio)
        {
            OcultarError();
            precio = 0;

            string nombre = TxtNombre.Text.Trim();
            string textoPrecio = TxtPrecio.Text.Trim();
            string descripcion = TxtDescripcion.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarError(
                    "Ingresa el nombre del tratamiento.");

                TxtNombre.Focus();
                return false;
            }

            if (nombre.Length > 100)
            {
                MostrarError(
                    "El nombre no puede superar los 100 caracteres.");

                TxtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(textoPrecio))
            {
                MostrarError(
                    "Ingresa el precio base del tratamiento.");

                TxtPrecio.Focus();
                return false;
            }

            string normalizado = textoPrecio.Replace(',', '.');

            if (!decimal.TryParse(
                normalizado,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out precio))
            {
                MostrarError(
                    "Ingresa un precio válido, por ejemplo: 350.00.");

                TxtPrecio.Focus();
                return false;
            }

            if (precio < 0)
            {
                MostrarError(
                    "El precio no puede ser negativo.");

                TxtPrecio.Focus();
                return false;
            }

            if (precio > 99999999.99m)
            {
                MostrarError(
                    "El precio supera el límite permitido.");

                TxtPrecio.Focus();
                return false;
            }

            if (decimal.Round(precio, 2) != precio)
            {
                MostrarError(
                    "El precio solo puede tener dos decimales.");

                TxtPrecio.Focus();
                return false;
            }

            if (descripcion.Length > 200)
            {
                MostrarError(
                    "La descripción no puede superar los 200 caracteres.");

                TxtDescripcion.Focus();
                return false;
            }

            return true;
        }

        // GUARDAR CAMBIOS
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
        private void ChkActivo_Checked(
            object sender,
            RoutedEventArgs e)
        {
            ActualizarTextoEstado();
            OcultarError();
        }

        private void ChkActivo_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            ActualizarTextoEstado();
            OcultarError();
        }

        // MOVER VENTANA
        private void Barra_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
