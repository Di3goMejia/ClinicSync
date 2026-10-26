
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

            TxtNombre.Text = _tratamiento.Nombre;

            TxtPrecio.Text = _tratamiento.Precio.ToString(
                "0.00",
                CultureInfo.InvariantCulture);

            TxtDescripcion.Text = _tratamiento.Descripcion;
            ChkActivo.IsChecked = _tratamiento.Activo;

            ActualizarTextoEstado();
        }

        private void ActualizarTextoEstado()
        {
            TxtEstado.Text = ChkActivo.IsChecked == true
                ? "Habilitado"
                : "Deshabilitado";
        }

        private void MostrarError(string mensaje)
        {
            TxtErrorGeneral.Text = mensaje;
            TxtErrorGeneral.Visibility = Visibility.Visible;
        }

        private void OcultarError()
        {
            TxtErrorGeneral.Text = string.Empty;
            TxtErrorGeneral.Visibility = Visibility.Collapsed;
        }

        private void TxtNombre_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            OcultarError();
        }

        private void TxtPrecio_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            OcultarError();
        }

        private void TxtDescripcion_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            OcultarError();
        }

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

        private bool ValidarFormulario(out decimal precio)
        {
            OcultarError();
            precio = 0;

            string nombre = TxtNombre.Text.Trim();
            string textoPrecio = TxtPrecio.Text.Trim();
            string descripcion = TxtDescripcion.Text.Trim();

            // Primero, validar el nombre.
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarError("Ingresa el nombre del tratamiento.");
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

            // Después, validar el precio.
            if (string.IsNullOrWhiteSpace(textoPrecio))
            {
                MostrarError("Ingresa el precio base del tratamiento.");
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
                MostrarError("El precio no puede ser negativo.");
                TxtPrecio.Focus();
                return false;
            }

            if (precio > 99999999.99m)
            {
                MostrarError("El precio supera el límite permitido.");
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

            // Finalmente, validar la descripción opcional.
            if (descripcion.Length > 200)
            {
                MostrarError(
                    "La descripción no puede superar los 200 caracteres.");

                TxtDescripcion.Focus();
                return false;
            }

            return true;
        }

        private async void BtnGuardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_guardando)
                return;

            OcultarError();

            if (!ValidarFormulario(out decimal precio))
                return;

            string nombre = TxtNombre.Text.Trim();
            string descripcion = TxtDescripcion.Text.Trim();
            bool activo = ChkActivo.IsChecked == true;

            _guardando = true;
            BtnGuardar.IsEnabled = false;

            try
            {
                bool cambioDatos =
                    nombre != _tratamiento.Nombre ||
                    precio != _tratamiento.Precio ||
                    descripcion != (_tratamiento.Descripcion ?? "");

                bool cambioEstado =
                    activo != _tratamiento.Activo;

                if (cambioDatos)
                {
                    await _tratamientosService.ModificarAsync(
                        _tratamiento.IdTratamiento,
                        nombre,
                        precio,
                        descripcion);
                }

                if (cambioEstado)
                {
                    await _tratamientosService.CambiarEstadoAsync(
                        _tratamiento.IdTratamiento,
                        activo);
                }

                _tratamiento.Nombre = nombre;
                _tratamiento.Descripcion = descripcion;
                _tratamiento.Precio = precio;
                _tratamiento.Activo = activo;

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorAlGuardar = false;

                if (ex.Message.Contains(
                    "Ya existe un tratamiento con ese nombre",
                    StringComparison.OrdinalIgnoreCase))
                {
                    MostrarError("El tratamiento ya existe.");
                }
                else
                {
                    MostrarError(
                        "No se pudo actualizar el tratamiento. Verifica la conexión e inténtalo nuevamente.");
                }
            }
            finally
            {
                _guardando = false;
                BtnGuardar.IsEnabled = true;
            }
        }

        private void ChkActivo_Checked(
            object sender,
            RoutedEventArgs e)
        {
            ActualizarTextoEstado();
        }

        private void ChkActivo_Unchecked(
            object sender,
            RoutedEventArgs e)
        {
            ActualizarTextoEstado();
        }

        private void Barra_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
