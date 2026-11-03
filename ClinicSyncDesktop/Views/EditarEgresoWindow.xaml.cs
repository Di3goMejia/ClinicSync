using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

namespace ClinicSyncDesktop.Views
{
    public partial class EditarEgresoWindow : Window
    {
        private readonly int _idEgreso;
        private readonly EgresosApiService _servicio;
        private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;
        private bool _guardando;

        public EditarEgresoWindow(int idEgreso, EgresosApiService servicio)
        {
            InitializeComponent();
            _idEgreso = idEgreso;
            _servicio = servicio;
            Loaded += EditarEgresoWindow_Loaded;
        }

        private async void EditarEgresoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Egreso egreso = await _servicio.ObtenerAsync(_idEgreso);
                dpFecha.SelectedDate = egreso.Fecha;
                txtHora.Text = egreso.Fecha.ToString("HH:mm", Cultura);
                txtConcepto.Text = egreso.Concepto;
                txtMonto.Text = egreso.Monto.ToString("0.00", Cultura);
                foreach (System.Windows.Controls.ComboBoxItem item in cmbMedio.Items)
                {
                    if (item.Content?.ToString() == egreso.MedioPago)
                    {
                        cmbMedio.SelectedItem = item;
                        break;
                    }
                }
                txtConcepto.IsReadOnly = egreso.Tipo == "MATERIALES";
            }
            catch (Exception ex)
            {
                txtError.Text = "No se pudo cargar el egreso: " + ex.Message;
                btnGuardar.IsEnabled = false;
            }
        }

        private bool TryObtenerFechaHora(out DateTime fecha)
        {
            fecha = default;
            if (dpFecha.SelectedDate is not DateTime fechaSeleccionada ||
                !TimeSpan.TryParseExact(txtHora.Text.Trim(), @"hh\:mm", Cultura, out TimeSpan hora))
            {
                return false;
            }

            fecha = fechaSeleccionada.Date.Add(hora);
            return true;
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (_guardando)
                return;

            txtError.Text = "";
            if (!TryObtenerFechaHora(out DateTime fecha) || fecha > DateTime.Now)
            {
                txtError.Text = "Ingrese una fecha y hora válidas que no sean futuras.";
                return;
            }

            if (cmbMedio.SelectedItem is not System.Windows.Controls.ComboBoxItem medio ||
                !decimal.TryParse(txtMonto.Text.Trim(), NumberStyles.Number, Cultura, out decimal monto) ||
                monto <= 0)
            {
                txtError.Text = "Revise el medio de pago y el monto.";
                return;
            }

            _guardando = true;
            btnGuardar.IsEnabled = false;
            try
            {
                await _servicio.ActualizarAsync(
                    _idEgreso,
                    fecha,
                    txtConcepto.Text.Trim(),
                    medio.Content?.ToString() ?? "",
                    monto);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                txtError.Text = "No se pudo actualizar el egreso: " + ex.Message;
                btnGuardar.IsEnabled = true;
                _guardando = false;
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
