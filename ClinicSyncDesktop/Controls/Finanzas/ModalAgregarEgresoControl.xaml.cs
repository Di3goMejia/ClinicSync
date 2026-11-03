using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Controls.Finanzas
{
    public partial class ModalAgregarEgresoControl : UserControl
    {
        public event EventHandler<TransaccionFinanciera>? EgresoAgregado;
        public event EventHandler? CerrarClick;
        public event EventHandler<string>? ConfirmarDescarte;
        public event EventHandler<string>? AvisoRequerido;
        public event EventHandler? AbrirSelectorMateriales;
        public event EventHandler? ErrorSimuladoClick;

        public ModalAgregarEgresoControl()
        {
            InitializeComponent();
        }

        public void Limpiar()
        {
            CmbTipo.SelectedIndex = 0;
            TxtFecha.Text = DateTime.Now.ToString("yyyy/MM/dd");

            TxtConcepto.Text = "Escribir...";
            TxtConcepto.Foreground = (SolidColorBrush)FindResource("ColorTextoClaro");

            if (TxtMaterialSeleccionado != null)
            {
                TxtMaterialSeleccionado.Text = "Seleccione un material";
                TxtMaterialSeleccionado.Foreground = (SolidColorBrush)FindResource("ColorTextoClaro");
            }

            if (BrdConceptoTexto != null && BrdConceptoMaterial != null)
            {
                BrdConceptoTexto.Visibility = Visibility.Visible;
                BrdConceptoMaterial.Visibility = Visibility.Collapsed;
            }

            CmbMedioPago.SelectedIndex = 0;
            TxtMonto.Text = "0.00";
        }

        public void SetMaterial(string nombreMaterial)
        {
            if (TxtMaterialSeleccionado != null)
            {
                TxtMaterialSeleccionado.Text = nombreMaterial;
                TxtMaterialSeleccionado.Foreground = (SolidColorBrush)FindResource("ColorTexto");
            }
            if (TxtConcepto != null)
            {
                TxtConcepto.Text = nombreMaterial;
            }
        }

        private void CmbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void BrdConceptoMaterial_Click(object sender, MouseButtonEventArgs e)
        {
            AbrirSelectorMateriales?.Invoke(this, EventArgs.Empty);
        }

        private void BtnPlusMaterial_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            AbrirSelectorMateriales?.Invoke(this, EventArgs.Empty);
        }

        private void TxtConcepto_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtConcepto.Text == "Escribir...")
            {
                TxtConcepto.Text = "";
                TxtConcepto.Foreground = (SolidColorBrush)FindResource("ColorTexto");
            }
        }

        private void TxtConcepto_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtConcepto.Text))
            {
                TxtConcepto.Text = "Escribir...";
                TxtConcepto.Foreground = (SolidColorBrush)FindResource("ColorTextoClaro");
            }
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
            string concepto = TxtConcepto.Text.Trim();
            string material = TxtMaterialSeleccionado?.Text?.Trim() ?? "";
            string montoTexto = TxtMonto.Text.Trim();
            int medioPagoIdx = CmbMedioPago.SelectedIndex;

            bool tieneCambios = CmbTipo.SelectedIndex > 0 ||
                                (!string.IsNullOrWhiteSpace(concepto) && concepto != "Escribir...") ||
                                (!string.IsNullOrWhiteSpace(material) && material != "Seleccione un material") ||
                                (!string.IsNullOrWhiteSpace(montoTexto) && montoTexto != "0.00" && montoTexto != "0") ||
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
            // Simulación de error si Shift está presionado
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                ErrorSimuladoClick?.Invoke(this, EventArgs.Empty);
                return;
            }

            string tipo = (CmbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string fecha = TxtFecha.Text.Trim();
            string medioPago = (CmbMedioPago.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            // Validación de campo obligatorio: Tipo
            if (CmbTipo.SelectedIndex <= 0 || string.IsNullOrWhiteSpace(tipo) || tipo == "Seleccione una opción")
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
                if (string.IsNullOrWhiteSpace(concepto) || concepto == "Escribir...")
                {
                    AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                    TxtConcepto.Focus();
                    return;
                }
            }

            // Validación de campo obligatorio: Medio de pago
            if (CmbMedioPago.SelectedIndex <= 0 || string.IsNullOrWhiteSpace(medioPago) || medioPago == "Seleccione una opción")
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

            var nuevo = new TransaccionFinanciera
            {
                Fecha = fecha,
                Tipo = tipo,
                Concepto = concepto,
                MedioPago = medioPago,
                Total = monto,
                EsIngreso = false,
                Activo = true
            };

            EgresoAgregado?.Invoke(this, nuevo);
        }
    }
}
