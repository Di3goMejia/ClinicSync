using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Controls.Finanzas
{
    public partial class ModalEditarEgresoControl : UserControl
    {
        public event EventHandler<TransaccionFinanciera>? GuardarClick;
        public event EventHandler? CerrarClick;
        public event EventHandler<string>? ConfirmarDescarte;
        public event EventHandler<string>? AvisoRequerido;
        public event EventHandler? AbrirSelectorMateriales;
        public event EventHandler? ErrorSimuladoClick;

        private TransaccionFinanciera? _transaccionActual;

        // Variables para detección de modificaciones
        private bool _origActivo;
        private string _origTipo = "";
        private string _origFecha = "";
        private string _origConcepto = "";
        private string _origMedioPago = "";
        private decimal _origMonto;

        public ModalEditarEgresoControl()
        {
            InitializeComponent();
        }

        public void Cargar(TransaccionFinanciera transaccion)
        {
            _transaccionActual = transaccion;

            _origActivo = transaccion.Activo;
            _origTipo = transaccion.Tipo ?? "";
            _origFecha = transaccion.Fecha ?? "";
            _origConcepto = transaccion.Concepto ?? "";
            _origMedioPago = transaccion.MedioPago ?? "";
            _origMonto = transaccion.Total;

            ChkActivo.IsChecked = transaccion.Activo;
            TxtFecha.Text = transaccion.Fecha ?? "";
            TxtConcepto.Text = transaccion.Concepto ?? "";
            if (TxtMaterialSeleccionado != null)
            {
                TxtMaterialSeleccionado.Text = string.IsNullOrWhiteSpace(transaccion.Concepto)
                    ? "Seleccione un material"
                    : transaccion.Concepto;
            }
            TxtMonto.Text = transaccion.Total.ToString("0.00");

            // Seleccionar tipo
            bool encontradoTipo = false;
            foreach (ComboBoxItem item in CmbTipo.Items)
            {
                if (string.Equals(item.Content?.ToString(), transaccion.Tipo, StringComparison.OrdinalIgnoreCase))
                {
                    CmbTipo.SelectedItem = item;
                    encontradoTipo = true;
                    break;
                }
            }
            if (!encontradoTipo && CmbTipo.Items.Count > 0)
            {
                CmbTipo.SelectedIndex = 0;
            }

            ActualizarVisibilidadConcepto();

            // Seleccionar medio de pago
            bool encontradoPago = false;
            foreach (ComboBoxItem item in CmbMedioPago.Items)
            {
                if (string.Equals(item.Content?.ToString(), transaccion.MedioPago, StringComparison.OrdinalIgnoreCase))
                {
                    CmbMedioPago.SelectedItem = item;
                    encontradoPago = true;
                    break;
                }
            }
            if (!encontradoPago && CmbMedioPago.Items.Count > 0)
            {
                CmbMedioPago.SelectedIndex = 0;
            }
        }

        public void SetMaterial(string nombreMaterial)
        {
            if (TxtMaterialSeleccionado != null)
            {
                TxtMaterialSeleccionado.Text = nombreMaterial;
            }
            if (TxtConcepto != null)
            {
                TxtConcepto.Text = nombreMaterial;
            }
        }

        private void ActualizarVisibilidadConcepto()
        {
            if (BrdConceptoTexto == null || BrdConceptoMaterial == null) return;

            string tipo = (CmbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            if (string.Equals(tipo, "Materiales", StringComparison.OrdinalIgnoreCase))
            {
                BrdConceptoTexto.Visibility = Visibility.Collapsed;
                BrdConceptoMaterial.Visibility = Visibility.Visible;
            }
            else
            {
                BrdConceptoTexto.Visibility = Visibility.Visible;
                BrdConceptoMaterial.Visibility = Visibility.Collapsed;
            }
        }

        private void CmbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarVisibilidadConcepto();
        }

        private void BrdConceptoMaterial_Click(object sender, MouseButtonEventArgs e)
        {
            AbrirSelectorMateriales?.Invoke(this, EventArgs.Empty);
        }

        private void BtnPlusMaterial_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            AbrirSelectorMateriales?.Invoke(this, EventArgs.Empty);
        }

        private void BtnDropdownTipo_Click(object sender, MouseButtonEventArgs e)
        {
            CmbTipo.IsDropDownOpen = true;
        }

        private void BtnDropdownMedioPago_Click(object sender, MouseButtonEventArgs e)
        {
            CmbMedioPago.IsDropDownOpen = true;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            string tipo = (CmbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string fecha = TxtFecha.Text.Trim();
            string medioPago = (CmbMedioPago.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            decimal.TryParse(TxtMonto.Text.Trim(), out decimal monto);

            string concepto = string.Equals(tipo, "Materiales", StringComparison.OrdinalIgnoreCase)
                ? (TxtMaterialSeleccionado?.Text?.Trim() ?? "")
                : TxtConcepto.Text.Trim();

            bool modificado = (ChkActivo.IsChecked != _origActivo) ||
                              !string.Equals(tipo, _origTipo, StringComparison.OrdinalIgnoreCase) ||
                              !string.Equals(fecha, _origFecha, StringComparison.Ordinal) ||
                              !string.Equals(concepto, _origConcepto, StringComparison.Ordinal) ||
                              !string.Equals(medioPago, _origMedioPago, StringComparison.OrdinalIgnoreCase) ||
                              (monto != _origMonto);

            if (modificado)
            {
                ConfirmarDescarte?.Invoke(this, "¿Seguro que desea\ndescartar esta edición?");
                return;
            }

            CerrarClick?.Invoke(this, EventArgs.Empty);
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Simular error al guardar cambios (Imagen 1) si Shift está presionado
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                ErrorSimuladoClick?.Invoke(this, EventArgs.Empty);
                return;
            }

            string tipo = (CmbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string fecha = TxtFecha.Text.Trim();
            string medioPago = (CmbMedioPago.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            // Validación de campo obligatorio: Tipo
            if (string.IsNullOrWhiteSpace(tipo) || tipo == "Seleccione una opción")
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                CmbTipo.IsDropDownOpen = true;
                return;
            }

            // Validación de campo obligatorio: Fecha
            if (string.IsNullOrWhiteSpace(fecha))
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                TxtFecha.Focus();
                return;
            }

            string concepto = "";
            if (string.Equals(tipo, "Materiales", StringComparison.OrdinalIgnoreCase))
            {
                concepto = TxtMaterialSeleccionado?.Text?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(concepto) || concepto == "Seleccione un material")
                {
                    AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                    AbrirSelectorMateriales?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
            else
            {
                concepto = TxtConcepto.Text.Trim();
                if (string.IsNullOrWhiteSpace(concepto))
                {
                    AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                    TxtConcepto.Focus();
                    return;
                }
            }

            // Validación de campo obligatorio: Medio de pago
            if (string.IsNullOrWhiteSpace(medioPago) || medioPago == "Seleccione una opción")
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                CmbMedioPago.IsDropDownOpen = true;
                return;
            }

            // Validación de campo obligatorio: Monto
            if (string.IsNullOrWhiteSpace(TxtMonto.Text) || !decimal.TryParse(TxtMonto.Text.Trim(), out decimal monto) || monto < 0)
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                TxtMonto.Focus();
                TxtMonto.SelectAll();
                return;
            }

            if (_transaccionActual == null)
            {
                _transaccionActual = new TransaccionFinanciera { EsIngreso = false };
            }

            _transaccionActual.Activo = ChkActivo.IsChecked == true;
            _transaccionActual.Tipo = tipo;
            _transaccionActual.Fecha = fecha;
            _transaccionActual.Concepto = concepto;
            _transaccionActual.MedioPago = medioPago;
            _transaccionActual.Total = monto;

            GuardarClick?.Invoke(this, _transaccionActual);
        }
    }
}
