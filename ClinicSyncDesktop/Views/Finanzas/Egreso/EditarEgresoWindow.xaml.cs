
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    public partial class EditarEgresoWindow : Window
    {
        private readonly int _idEgreso;
        private readonly EgresosApiService _servicio;

        private static readonly CultureInfo Cultura =
            CultureInfo.InvariantCulture;

        private bool _guardando;

        public EditarEgresoWindow(
            int idEgreso,
            EgresosApiService servicio)
        {
            InitializeComponent();

            _idEgreso = idEgreso;
            _servicio = servicio;

            Loaded += EditarEgresoWindow_Loaded;
        }

        // CARGAR LOS DATOS DEL EGRESO
        private async void EditarEgresoWindow_Loaded(
            object sender, RoutedEventArgs e)
        {
            txtError.Text = "";
            btnGuardar.IsEnabled = false;

            try
            {
                Egreso egreso =
                    await _servicio.ObtenerAsync(_idEgreso);

                // FECHA
                dpFecha.SelectedDate = egreso.Fecha;

                // HORA
                txtHora.Text =
                    egreso.Fecha.ToString("HH:mm", Cultura);

                // CONCEPTO
                txtConcepto.Text = egreso.Concepto;

                // MONTO
                txtMonto.Text =
                    egreso.Monto.ToString("0.00", Cultura);

                // MEDIO DE PAGO
                cmbMedio.SelectedItem = null;

                foreach (ComboBoxItem item in cmbMedio.Items)
                {
                    if (string.Equals(
                        item.Content?.ToString(),
                        egreso.MedioPago,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        cmbMedio.SelectedItem = item;
                        break;
                    }
                }

                // CONSERVAR RESTRICCIÓN DE MATERIALES
                txtConcepto.IsReadOnly =
                    egreso.Tipo == "MATERIALES";

                // ESTADO ACTUAL DEL EGRESO
                chkActivo.IsChecked = egreso.Activo;

                btnGuardar.IsEnabled = true;
            }
            catch (Exception ex)
            {
                txtError.Text =
                    "No se pudo cargar el egreso: " +
                    ex.Message;

                btnGuardar.IsEnabled = false;
            }
        }

        // OBTENER FECHA Y HORA
        private bool TryObtenerFechaHora(out DateTime fecha)
        {
            fecha = default;

            if (dpFecha.SelectedDate is not DateTime fechaSeleccionada ||
                !TimeSpan.TryParseExact(
                    txtHora.Text.Trim(),
                    @"hh\:mm",
                    Cultura,
                    out TimeSpan hora))
            {
                return false;
            }

            fecha = fechaSeleccionada.Date.Add(hora);

            return true;
        }

        // GUARDAR CAMBIOS

        private async void Guardar_Click(
            object sender, RoutedEventArgs e)
        {
            if (_guardando)
                return;

            txtError.Text = "";

            // VALIDAR FECHA Y HORA
            if (!TryObtenerFechaHora(out DateTime fecha) ||
                fecha > DateTime.Now)
            {
                txtError.Text =
                    "Ingrese una fecha y hora válidas " +
                    "que no sean futuras.";
                return;
            }

            // VALIDAR MEDIO DE PAGO
            if (cmbMedio.SelectedItem is not ComboBoxItem medio)
            {
                txtError.Text = "Seleccione un medio de pago.";
                return;
            }

            // VALIDAR MONTO
            if (!decimal.TryParse(
                    txtMonto.Text.Trim(),
                    NumberStyles.Number,
                    Cultura,
                    out decimal monto) ||
                monto <= 0)
            {
                txtError.Text =
                    "El monto debe ser mayor que 0.";
                return;
            }

            // VALIDAR CONCEPTO
            if (string.IsNullOrWhiteSpace(txtConcepto.Text))
            {
                txtError.Text =
                    "Ingrese un concepto válido.";
                return;
            }

            // OBTENER ESTADO DEL EGRESO
            bool activo = chkActivo.IsChecked == true;

            _guardando = true;
            btnGuardar.IsEnabled = false;

            try
            {
                // ENVIAR LOS DATOS AL SERVIDOR
                await _servicio.ActualizarAsync(
                    _idEgreso,
                    fecha,
                    txtConcepto.Text.Trim(),
                    medio.Content?.ToString() ?? "",
                    monto,
                    activo);

                // CERRAR SOLO SI LA ACTUALIZACIÓN TERMINÓ SIN ERROR
                DialogResult = true;
            }
            catch (Exception ex)
            {
                txtError.Text =
                    "No se pudo actualizar el egreso: " +
                    ex.Message;
            }
            finally
            {
                _guardando = false;

                if (DialogResult != true)
                    btnGuardar.IsEnabled = true;
            }
        }


        // CANCELAR EDICIÓN
        private void Cancelar_Click(
            object sender, RoutedEventArgs e)
        {
            if (_guardando)
                return;

            DialogResult = false;
        }
    }
}

