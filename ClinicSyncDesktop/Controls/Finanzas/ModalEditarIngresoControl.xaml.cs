using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Controls.Finanzas
{
    public partial class ModalEditarIngresoControl : UserControl
    {
        public event EventHandler<TransaccionFinanciera>? GuardarClick;
        public event EventHandler? CerrarClick;
        public event EventHandler<string>? ConfirmarDescarte;
        public event EventHandler? ImprimirFacturaClick;
        public event EventHandler<string>? AvisoRequerido;
        public event EventHandler? ErrorSimuladoClick;

        private TransaccionFinanciera? _transaccionActual;
        private int _indicePaciente = 0;
        private int _indiceTratamiento = 0;

        // Variables para detección de modificaciones
        private bool _origActivo;
        private string _origPaciente = "";
        private string _origTratamiento = "";
        private string _origFecha = "";
        private decimal _origDescuento;
        private string _origMedioPago = "";

        public ModalEditarIngresoControl()
        {
            InitializeComponent();
        }

        public void Cargar(TransaccionFinanciera transaccion)
        {
            _transaccionActual = transaccion;

            _origActivo = transaccion.Activo;
            _origPaciente = transaccion.Paciente ?? "";
            _origTratamiento = transaccion.Tratamiento ?? "";
            _origFecha = transaccion.Fecha ?? "";
            _origDescuento = transaccion.Descuento;
            _origMedioPago = transaccion.MedioPago ?? "";

            ChkActivo.IsChecked = transaccion.Activo;
            TxtPaciente.Text = transaccion.Paciente ?? "";
            TxtTratamiento.Text = transaccion.Tratamiento ?? "";
            TxtFecha.Text = transaccion.Fecha ?? "";
            TxtDescuento.Text = transaccion.Descuento.ToString("0.00");

            // Seleccionar medio de pago en el ComboBox
            bool encontrado = false;
            foreach (ComboBoxItem item in CmbMedioPago.Items)
            {
                if (string.Equals(item.Content?.ToString(), transaccion.MedioPago, StringComparison.OrdinalIgnoreCase))
                {
                    CmbMedioPago.SelectedItem = item;
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado && CmbMedioPago.Items.Count > 0)
            {
                CmbMedioPago.SelectedIndex = 0;
            }

            TxtPaciente.Focus();
            TxtPaciente.SelectAll();
        }

        private void BtnSeleccionarPaciente_Click(object sender, MouseButtonEventArgs e)
        {
            var pacientes = CatalogoClinica.Pacientes;
            if (pacientes.Count > 0)
            {
                TxtPaciente.Text = pacientes[_indicePaciente % pacientes.Count];
                _indicePaciente = (_indicePaciente + 1) % pacientes.Count;
            }
        }

        private void BtnSeleccionarTratamiento_Click(object sender, MouseButtonEventArgs e)
        {
            var tratamientos = CatalogoClinica.Tratamientos;
            if (tratamientos.Count > 0)
            {
                TxtTratamiento.Text = tratamientos[_indiceTratamiento % tratamientos.Count];
                _indiceTratamiento = (_indiceTratamiento + 1) % tratamientos.Count;
            }
        }

        private void BtnDropdownMedioPago_Click(object sender, MouseButtonEventArgs e)
        {
            CmbMedioPago.IsDropDownOpen = true;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            string paciente = TxtPaciente.Text.Trim();
            string tratamiento = TxtTratamiento.Text.Trim();
            string fecha = TxtFecha.Text.Trim();
            string medioPago = (CmbMedioPago.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? CmbMedioPago.Text?.Trim() ?? "";
            decimal.TryParse(TxtDescuento.Text.Trim(), out decimal descuento);

            bool modificado = (ChkActivo.IsChecked != _origActivo) ||
                              !string.Equals(paciente, _origPaciente, StringComparison.Ordinal) ||
                              !string.Equals(tratamiento, _origTratamiento, StringComparison.Ordinal) ||
                              !string.Equals(fecha, _origFecha, StringComparison.Ordinal) ||
                              (descuento != _origDescuento) ||
                              !string.Equals(medioPago, _origMedioPago, StringComparison.OrdinalIgnoreCase);

            if (modificado)
            {
                ConfirmarDescarte?.Invoke(this, "¿Seguro que desea\ndescartar esta edición?");
                return;
            }

            CerrarClick?.Invoke(this, EventArgs.Empty);
        }

        private void BtnImprimirFactura_Click(object sender, RoutedEventArgs e)
        {
            ImprimirFacturaClick?.Invoke(this, EventArgs.Empty);
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Simular error al guardar cambios si Shift está presionado
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                ErrorSimuladoClick?.Invoke(this, EventArgs.Empty);
                return;
            }

            string paciente = TxtPaciente.Text.Trim();
            string tratamiento = TxtTratamiento.Text.Trim();
            string fecha = TxtFecha.Text.Trim();
            string medioPago = (CmbMedioPago.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? CmbMedioPago.Text?.Trim() ?? "";

            // Validación de campo obligatorio: Paciente
            if (string.IsNullOrWhiteSpace(paciente) || paciente == "Seleccione un paciente")
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                TxtPaciente.Focus();
                return;
            }

            // Validación de campo obligatorio: Fecha
            if (string.IsNullOrWhiteSpace(fecha))
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                TxtFecha.Focus();
                return;
            }

            // Validación de campo obligatorio: Tratamiento
            if (string.IsNullOrWhiteSpace(tratamiento) || tratamiento == "Seleccione un tratamiento")
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                TxtTratamiento.Focus();
                return;
            }

            // Validación de campo obligatorio: Descuento
            if (string.IsNullOrWhiteSpace(TxtDescuento.Text) || !decimal.TryParse(TxtDescuento.Text.Trim(), out decimal descuento) || descuento < 0)
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                TxtDescuento.Focus();
                TxtDescuento.SelectAll();
                return;
            }

            // Validación de campo obligatorio: Medio de pago
            if (string.IsNullOrWhiteSpace(medioPago) || medioPago == "Seleccione una opción")
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                CmbMedioPago.IsDropDownOpen = true;
                return;
            }

            if (_transaccionActual == null)
            {
                _transaccionActual = new TransaccionFinanciera { EsIngreso = true };
            }

            // Recalcular Total dinámicamente si cambió el tratamiento o descuento
            decimal basePrice = (_transaccionActual.Total > 0
                ? _transaccionActual.Total + _origDescuento
                : CatalogoClinica.ObtenerPrecioBaseTratamiento(tratamiento));

            decimal totalCalculado = Math.Max(0m, basePrice - descuento);

            _transaccionActual.Activo = ChkActivo.IsChecked == true;
            _transaccionActual.Paciente = paciente;
            _transaccionActual.Tratamiento = tratamiento;
            _transaccionActual.Concepto = tratamiento;
            _transaccionActual.Fecha = fecha;
            _transaccionActual.Descuento = descuento;
            _transaccionActual.MedioPago = medioPago;
            _transaccionActual.Total = totalCalculado;

            CatalogoClinica.RegistrarPaciente(paciente);
            CatalogoClinica.RegistrarPrecio(tratamiento, basePrice);

            GuardarClick?.Invoke(this, _transaccionActual);
        }
    }
}
