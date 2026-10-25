using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClinicSyncDesktop.Views
{
    public partial class EditarTratamientoWindow : Window
    {
        private readonly TratamientosApiService _tratamientosService;
        private readonly Tratamiento _tratamiento;
        private bool _guardando;

        public EditarTratamientoWindow(Tratamiento tr, TratamientosApiService svc)
        {
            InitializeComponent();

            _tratamiento = tr;
            _tratamientosService = svc;

            TxtNombre.Text = _tratamiento.Nombre;
            TxtPrecio.Text = _tratamiento.Precio.ToString("0.00");
            TxtDescripcion.Text = _tratamiento.Descripcion;
            ChkActivo.IsChecked = _tratamiento.Activo;

            ActualizarTextoEstado();
        }

        private void ActualizarTextoEstado()
        {
            TxtEstado.Text = ChkActivo.IsChecked == true ? "Habilitado" : "Deshabilitado";
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
                nombre      != _tratamiento.Nombre ||
                precio      != _tratamiento.Precio.ToString("0.00") ||
                descripcion != (_tratamiento.Descripcion ?? "") ||
                activo      != _tratamiento.Activo;

            if (tieneCambios)
            {
                bool descartar = MensajeWindow.MostrarConfirmacion(
                    this,
                    "¿Seguro que desea descartar esta edición?"
                );

                // Si seleccionó "No", cancelar el cierre y permanecer editando.
                if (!descartar) 
                    return;
            }

            DialogResult = false;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (_guardando)
                return;

            if (_tratamientosService is null)
            {
                MensajeWindow.MostrarError(
                    this,
                    "La conexión con la API no está configurada.");

                return;
            }

            string nombre = TxtNombre.Text.Trim();
            string textoPrecio = TxtPrecio.Text.Trim();
            string descripcion = TxtDescripcion.Text.Trim();

            // Validar nombre obligatorio
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "Ingresa el nombre del tratamiento.");

                TxtNombre.Focus();
                return;
            }

            // Validar longitud de nombre.
            if (nombre.Length > 100)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "El nombre no puede superar los 100 caracteres.");

                TxtNombre.Focus();
                return;
            }

            // Validar precio obligatorio
            if (string.IsNullOrWhiteSpace(textoPrecio))
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "Ingresa el precio base del tratamiento.");

                TxtPrecio.Focus();
                return;
            }

            // Aceptar coma o punto decimal
            string precioNormalizado = textoPrecio.Replace(',', '.');

            if (!decimal.TryParse(
                    precioNormalizado,
                    NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out decimal precio))
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "Ingresa un precio válido. Puedes utilizar hasta dos decimales, por ejemplo: 350.00.");

                TxtPrecio.Focus();
                return;
            }

            if (precio < 0)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "El precio no puede ser negativo.");

                TxtPrecio.Focus();
                return;
            }

            if (precio > 99999999.99m)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "El precio supera el límite permitido.");

                TxtPrecio.Focus();
                return;
            }

            if (decimal.Round(precio, 2) != precio)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "El precio solo puede tener dos decimales.");

                TxtPrecio.Focus();
                return;
            }

            // La descripción es opcional
            if (descripcion.Length > 200)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "La descripción no puede superar los 200 caracteres.");

                TxtDescripcion.Focus();
                return;
            }

            _guardando = true;
            BtnGuardar.IsEnabled = false;

            try
            {
                bool cambioDatos =
                    nombre != _tratamiento.Nombre ||
                    precio != _tratamiento.Precio ||
                    descripcion != (_tratamiento.Descripcion ?? "");

                if (cambioDatos)
                    await _tratamientosService.ModificarAsync(
                        _tratamiento.IdTratamiento,
                        nombre,
                        precio,
                        descripcion);
                
                bool activo = ChkActivo.IsChecked == true;

                // Detectar si también se cambió el estado.
                if (activo != _tratamiento.Activo)
                {
                    await _tratamientosService.CambiarEstadoAsync(
                        _tratamiento.IdTratamiento, activo);
                }

                MensajeWindow.MostrarExito(
                    this,
                    "Los cambios se guardaron con éxito.");

                _tratamiento.Nombre = nombre;
                _tratamiento.Descripcion = descripcion;
                _tratamiento.Precio = precio;
                _tratamiento.Activo = activo;

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MensajeWindow.MostrarError(
                    this, ex.Message);
            }
            finally
            {
                _guardando = false;
                BtnGuardar.IsEnabled = true;
            }

        }

        private void ChkActivo_Checked(object sender, RoutedEventArgs e)
        {
            ActualizarTextoEstado();
        }

        private void ChkActivo_Unchecked(object sender, RoutedEventArgs e)
        {
            ActualizarTextoEstado();
        }

        private void Barra_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }
    }
}
