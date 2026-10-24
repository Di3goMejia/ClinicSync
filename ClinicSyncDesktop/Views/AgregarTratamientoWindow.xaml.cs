using ClinicSyncDesktop.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

namespace ClinicSyncDesktop.Views
{
    public partial class AgregarTratamientoWindow : Window
    {
        private readonly TratamientosApiService? _tratamientosService;
        private bool _guardando;

        public AgregarTratamientoWindow()
        {
            InitializeComponent();
        }

        public AgregarTratamientoWindow(
            TratamientosApiService tratamientosService)
            : this()
        {
            _tratamientosService = tratamientosService;
        }

        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void Agregar_Click(
            object sender,
            RoutedEventArgs e)
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

            string nombre = txtNombre.Text.Trim();
            string textoPrecio = txtPrecio.Text.Trim();
            string descripcion = txtDescripcion.Text.Trim();

            // Validar nombre obligatorio
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "Ingresa el nombre del tratamiento.");

                txtNombre.Focus();
                return;
            }

            if (nombre.Length > 100)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "El nombre no puede superar los 100 caracteres.");

                txtNombre.Focus();
                return;
            }

            // Validar precio obligatorio
            if (string.IsNullOrWhiteSpace(textoPrecio))
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "Ingresa el precio base del tratamiento.");

                txtPrecio.Focus();
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

                txtPrecio.Focus();
                return;
            }

            if (precio < 0)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "El precio no puede ser negativo.");

                txtPrecio.Focus();
                return;
            }

            if (precio > 99999999.99m)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "El precio supera el límite permitido.");

                txtPrecio.Focus();
                return;
            }

            if (decimal.Round(precio, 2) != precio)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "El precio solo puede tener dos decimales.");

                txtPrecio.Focus();
                return;
            }

            // La descripción es opcional
            if (descripcion.Length > 200)
            {
                MensajeWindow.MostrarAviso(
                    this,
                    "La descripción no puede superar los 200 caracteres.");

                txtDescripcion.Focus();
                return;
            }

            _guardando = true;
            btnAgregar.IsEnabled = false;

            try
            {
                int idTratamiento =
                    await _tratamientosService.RegistrarAsync(
                        nombre,
                        precio,
                        string.IsNullOrWhiteSpace(descripcion)
                            ? null
                            : descripcion);

                MensajeWindow.MostrarExito(
                    this,
                    "El tratamiento se agregó con éxito.");

                DialogResult = true;
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message;

                // Mensaje especial para nombres duplicados
                if (mensaje.Contains(
                    "Ya existe un tratamiento con ese nombre",
                    StringComparison.OrdinalIgnoreCase))
                {
                    MensajeWindow.MostrarInformacion(
                        this,
                        "El tratamiento ya existe.");
                }
                else
                {
                    MensajeWindow.MostrarError(
                        this,
                        mensaje);
                }
            }
            finally
            {
                _guardando = false;
                btnAgregar.IsEnabled = true;
            }
        }
    }
}
