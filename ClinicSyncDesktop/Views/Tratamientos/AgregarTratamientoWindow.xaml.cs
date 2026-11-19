
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSyncDesktop.Views
{
    public partial class AgregarTratamientoWindow : Window
    {
        private readonly ValidadorTratamiento _validador = new();
        private readonly Dictionary<string, Control> _controles;
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
            _controles = new Dictionary<string, Control>
            {
                [nameof(EntradaTratamiento.Nombre)] = TxtNombre,
                [nameof(EntradaTratamiento.TextoPrecio)] = TxtPrecio,
                [nameof(EntradaTratamiento.Descripcion)] = TxtDescripcion
            };
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

        private void Barra_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
