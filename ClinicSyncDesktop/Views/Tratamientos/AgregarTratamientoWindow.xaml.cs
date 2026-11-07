
using ClinicSyncDesktop.Services;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSyncDesktop.Views
{
    public partial class AgregarTratamientoWindow : Window
    {
        private readonly TratamientosApiService? _tratamientosService;
        private bool _guardando;

        public bool ErrorAlRegistrar { get; private set; }

        public string MensajeError { get; private set; } =
            "No se pudo registrar el tratamiento.";

        public AgregarTratamientoWindow(
            TratamientosApiService tratamientosService)
        {
            InitializeComponent();
            _tratamientosService = tratamientosService;
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

        private bool ValidarFormulario(out decimal precio)
        {
            OcultarError();
            precio = 0;

            string nombre = TxtNombre.Text.Trim();
            string textoPrecio = TxtPrecio.Text.Trim();
            string descripcion = TxtDescripcion.Text.Trim();

            // Validar primero el nombre.
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

            // Validar el precio obligatorio.
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

            // La descripción es opcional.
            if (descripcion.Length > 200)
            {
                MostrarError(
                    "La descripción no puede superar los 200 caracteres.");

                TxtDescripcion.Focus();
                return false;
            }

            return true;
        }

        private async void Agregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_guardando)
                return;

            OcultarError();

            if (_tratamientosService is null)
            {
                MostrarError(
                    "La conexión con la API no está configurada.");

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

        private void Barra_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
