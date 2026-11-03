using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Controls.Finanzas
{
    public partial class ModalAgregarIngresoControl : UserControl
    {
        public event EventHandler<TransaccionFinanciera>? IngresoAgregado;
        public event EventHandler? CerrarClick;
        public event EventHandler<string>? ConfirmarDescarte;
        public event EventHandler? AbrirSelectorMateriales;
        public event EventHandler<string>? AvisoRequerido;
        public event EventHandler? ErrorSimuladoClick;

        private int _indicePaciente = 0;
        private int _indiceTratamiento = 0;

        public ModalAgregarIngresoControl()
        {
            InitializeComponent();
        }

        public void Limpiar()
        {
            TxtPaciente.Text = "Seleccione un paciente";
            TxtPaciente.Foreground = (SolidColorBrush)FindResource("ColorTextoClaro");

            TxtFecha.Text = DateTime.Now.ToString("yyyy/MM/dd");

            TxtTratamiento.Text = "Seleccione un tratamiento";
            TxtTratamiento.Foreground = (SolidColorBrush)FindResource("ColorTextoClaro");

            TxtMateriales.Text = "Seleccionar";
            TxtMateriales.Foreground = (SolidColorBrush)FindResource("ColorTextoClaro");

            TxtDescuento.Text = "0.00";
            CmbMedioPago.SelectedIndex = 0;
        }

        public void SetMateriales(string materiales)
        {
            TxtMateriales.Text = string.IsNullOrWhiteSpace(materiales) ? "Seleccionar" : materiales;
            TxtMateriales.Foreground = string.IsNullOrWhiteSpace(materiales) || materiales == "Seleccionar"
                ? (SolidColorBrush)FindResource("ColorTextoClaro")
                : (SolidColorBrush)FindResource("ColorTexto");
        }

        private void TxtPlaceholder_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.Text == "Seleccione un paciente" || tb.Text == "Seleccione un tratamiento")
                {
                    tb.Text = "";
                    tb.Foreground = (SolidColorBrush)FindResource("ColorTexto");
                }
            }
        }

        private void TxtPlaceholder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    if (tb == TxtPaciente) tb.Text = "Seleccione un paciente";
                    else if (tb == TxtTratamiento) tb.Text = "Seleccione un tratamiento";

                    tb.Foreground = (SolidColorBrush)FindResource("ColorTextoClaro");
                }
            }
        }

        private void BtnSeleccionarPaciente_Click(object sender, MouseButtonEventArgs e)
        {
            var pacientes = CatalogoClinica.Pacientes;
            if (pacientes.Count > 0)
            {
                TxtPaciente.Text = pacientes[_indicePaciente % pacientes.Count];
                TxtPaciente.Foreground = (SolidColorBrush)FindResource("ColorTexto");
                _indicePaciente = (_indicePaciente + 1) % pacientes.Count;
            }
        }

        private void BtnSeleccionarTratamiento_Click(object sender, MouseButtonEventArgs e)
        {
            var tratamientos = CatalogoClinica.Tratamientos;
            if (tratamientos.Count > 0)
            {
                TxtTratamiento.Text = tratamientos[_indiceTratamiento % tratamientos.Count];
                TxtTratamiento.Foreground = (SolidColorBrush)FindResource("ColorTexto");
                _indiceTratamiento = (_indiceTratamiento + 1) % tratamientos.Count;
            }
        }

        private void BtnSeleccionarMateriales_Click(object sender, MouseButtonEventArgs e)
        {
            AbrirSelectorMateriales?.Invoke(this, EventArgs.Empty);
        }

        private void BtnDropdownMedioPago_Click(object sender, MouseButtonEventArgs e)
        {
            CmbMedioPago.IsDropDownOpen = true;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            string paciente = TxtPaciente.Text.Trim();
            string tratamiento = TxtTratamiento.Text.Trim();
            string materiales = TxtMateriales.Text.Trim();
            string descuento = TxtDescuento.Text.Trim();
            int medioPagoIdx = CmbMedioPago.SelectedIndex;

            bool tieneCambios = (!string.IsNullOrWhiteSpace(paciente) && paciente != "Seleccione un paciente") ||
                                (!string.IsNullOrWhiteSpace(tratamiento) && tratamiento != "Seleccione un tratamiento") ||
                                (!string.IsNullOrWhiteSpace(materiales) && materiales != "Seleccionar") ||
                                (!string.IsNullOrWhiteSpace(descuento) && descuento != "0.00" && descuento != "0") ||
                                medioPagoIdx > 0;

            if (tieneCambios)
            {
                ConfirmarDescarte?.Invoke(this, "¿Seguro que desea\ndescartar este registro?");
                return;
            }

            CerrarClick?.Invoke(this, EventArgs.Empty);
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            // Simular error si Shift está presionado
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                ErrorSimuladoClick?.Invoke(this, EventArgs.Empty);
                return;
            }

            string paciente = TxtPaciente.Text.Trim();
            string tratamiento = TxtTratamiento.Text.Trim();
            string fecha = TxtFecha.Text.Trim();
            string materiales = TxtMateriales.Text.Trim();
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

            // Validación de campo obligatorio: Materiales (asterisco en diseño)
            if (string.IsNullOrWhiteSpace(materiales) || materiales == "Seleccionar")
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                AbrirSelectorMateriales?.Invoke(this, EventArgs.Empty);
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
            if (CmbMedioPago.SelectedIndex <= 0 || string.IsNullOrWhiteSpace(medioPago) || medioPago == "Seleccione una opción")
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                CmbMedioPago.IsDropDownOpen = true;
                return;
            }

            // Cálculo funcional del Total a partir del precio base del tratamiento menos descuento
            decimal precioBase = CatalogoClinica.ObtenerPrecioBaseTratamiento(tratamiento);
            decimal totalCalculado = Math.Max(0m, precioBase - descuento);

            var nuevo = new TransaccionFinanciera
            {
                Fecha = fecha,
                Paciente = paciente,
                Tratamiento = tratamiento,
                Concepto = tratamiento,
                MedioPago = medioPago,
                Descuento = descuento,
                Materiales = materiales != "Seleccionar" ? materiales : "",
                Total = totalCalculado,
                EsIngreso = true,
                Activo = true
            };

            CatalogoClinica.RegistrarPaciente(paciente);
            CatalogoClinica.RegistrarPrecio(tratamiento, precioBase);

            IngresoAgregado?.Invoke(this, nuevo);
        }
    }
}
