using ClinicSyncDesktop.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    public partial class AgregarEgresoWindow : Window
    {
        private readonly EgresosApiService _egresosService;
        private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;
        private bool _formateando;
        private List<MaterialParaEgreso> _materiales = new();

        public AgregarEgresoWindow(EgresosApiService egresosService)
        {
            InitializeComponent();

            _egresosService = egresosService;

            // FECHA DE HOY POR DEFECTO
            dpFecha.SelectedDate = DateTime.Today;
            txtFecha.Text = DateTime.Today.ToString("yyyy/MM/dd", Cultura);
        }

        // MOVER LA VENTANA DESDE EL ENCABEZADO
        private void Encabezado_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // MENSAJES DE VALIDACIÓN
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            iconoError.Visibility = Visibility.Visible;
        }

        private void OcultarError()
        {
            txtError.Text = string.Empty;
            iconoError.Visibility = Visibility.Collapsed;
        }

        // ¿HAY DATOS ESCRITOS? (SIN CONTAR LA FECHA POR DEFECTO)
        private bool HayDatosEscritos() =>
            cmbTipo.SelectedIndex > 0 ||
            cmbMedio.SelectedIndex > 0 ||
            _materiales.Count > 0 ||
            !string.IsNullOrWhiteSpace(txtConcepto.Text) ||
            !string.IsNullOrWhiteSpace(txtMonto.Text);

        // CERRAR, PIDIENDO CONFIRMACIÓN SI HAY DATOS
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (HayDatosEscritos())
            {
                var respuesta = MessageBox.Show(
                    "¿Seguro que desea descartar este registro?",
                    "Cuidado",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (respuesta != MessageBoxResult.Yes)
                    return;
            }

            DialogResult = false;
        }

        // ===================== TIPO Y MATERIALES =====================

        private bool EsMateriales() =>
            ((cmbTipo.SelectedItem as ComboBoxItem)?.Tag as string)
                == "MATERIALES";

        // MATERIALES MUESTRA EL SELECTOR; SERVICIOS, EL TEXTO LIBRE
        private void cmbTipo_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (panelMateriales is null || txtConcepto is null)
                return;

            bool materiales = EsMateriales();

            panelMateriales.Visibility =
                materiales ? Visibility.Visible : Visibility.Collapsed;

            txtConcepto.Visibility =
                materiales ? Visibility.Collapsed : Visibility.Visible;

            OcultarError();
        }

        private void Materiales_Click(
            object sender,
            MouseButtonEventArgs e)
        {
            var ventana = new SeleccionarMaterialesWindow(_materiales)
            {
                Owner = this
            };

            if (ventana.ShowDialog() == true)
            {
                _materiales = ventana.Seleccionados;
                ActualizarTextoMateriales();
                OcultarError();
            }
        }

        private void ActualizarTextoMateriales()
        {
            if (_materiales.Count == 0)
            {
                txtMateriales.Text = "Seleccione un material";
                txtMateriales.Foreground =
                    new SolidColorBrush(Color.FromRgb(0x8A, 0x9A, 0x9E));
                return;
            }

            txtMateriales.Text = string.Join(
                ", ",
                _materiales.Select(m => $"{m.Nombre} ({m.Cantidad:0.##})"));

            txtMateriales.Foreground =
                new SolidColorBrush(Color.FromRgb(0x17, 0x47, 0x5A));
        }

        // ===================== FECHA =====================

        private bool TryObtenerFecha(out DateTime fecha) =>
            DateTime.TryParseExact(
                txtFecha.Text.Trim(),
                "yyyy/MM/dd",
                Cultura,
                DateTimeStyles.None,
                out fecha);

        // ESCRIBE SOLO DÍGITOS Y PONE LAS DIAGONALES SOLAS
        private void txtFecha_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            txtPlaceholderFecha.Visibility =
                string.IsNullOrEmpty(txtFecha.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            if (_formateando)
                return;

            _formateando = true;

            string digitos = new string(
                txtFecha.Text.Where(char.IsDigit).Take(8).ToArray());

            string formato = digitos;

            if (digitos.Length > 6)
                formato = digitos[..4] + "/" + digitos[4..6] + "/" + digitos[6..];
            else if (digitos.Length > 4)
                formato = digitos[..4] + "/" + digitos[4..];

            if (formato != txtFecha.Text)
            {
                txtFecha.Text = formato;
                txtFecha.CaretIndex = formato.Length;
            }

            // SINCRONIZAR EL CALENDARIO
            if (TryObtenerFecha(out DateTime fecha) &&
                dpFecha.SelectedDate != fecha)
            {
                dpFecha.SelectedDate = fecha;
            }

            _formateando = false;
        }

        private void txtFecha_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (txtFecha.Text.Length > 0 && !TryObtenerFecha(out _))
            {
                MostrarError(
                    "La fecha no es válida. Usa el formato AAAA/MM/DD.");
            }
        }

        private void Calendario_Click(
            object sender,
            RoutedEventArgs e)
        {
            dpFecha.IsDropDownOpen = true;
        }

        private void dpFecha_SelectedDateChanged(
            object? sender,
            SelectionChangedEventArgs e)
        {
            if (dpFecha.SelectedDate is DateTime fecha)
            {
                txtFecha.Text = fecha.ToString("yyyy/MM/dd", Cultura);
                OcultarError();
            }
        }

        // ===================== MONTO =====================

        // SOLO DÍGITOS Y UN PUNTO DECIMAL
        private void txtMonto_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            e.Handled =
                !Regex.IsMatch(e.Text, @"^[0-9.]+$") ||
                (e.Text.Contains('.') && txtMonto.Text.Contains('.'));
        }

        // ===================== AGREGAR =====================

        private async void Agregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            OcultarError();

            string? tipo =
                (cmbTipo.SelectedItem as ComboBoxItem)?.Tag as string;

            string? medio =
                (cmbMedio.SelectedItem as ComboBoxItem)?.Tag as string;

            bool esMateriales = tipo == "MATERIALES";

            string concepto = txtConcepto.Text.Trim();
            string textoMonto = txtMonto.Text.Trim();
            string textoFecha = txtFecha.Text.Trim();

            // CAMPOS OBLIGATORIOS
            if (tipo is null || medio is null ||
                textoFecha.Length == 0 ||
                textoMonto.Length == 0 ||
                (!esMateriales && concepto.Length == 0) ||
                (esMateriales && _materiales.Count == 0))
            {
                MostrarError("Se deben llenar los campos obligatorios.");
                return;
            }

            // FECHA VÁLIDA
            if (!TryObtenerFecha(out DateTime fecha))
            {
                MostrarError(
                    "La fecha no es válida. Usa el formato AAAA/MM/DD.");
                return;
            }

            // MONTO NUMÉRICO
            if (!decimal.TryParse(
                    textoMonto,
                    NumberStyles.AllowDecimalPoint,
                    Cultura,
                    out decimal monto))
            {
                MostrarError("El monto debe ser un valor numérico válido.");
                return;
            }

            monto = Math.Round(monto, 2);

            // MONTO MAYOR QUE CERO
            if (monto <= 0)
            {
                MostrarError("El monto debe ser mayor que cero.");
                return;
            }

            if (monto > 99999999.99m)
            {
                MostrarError("El monto es demasiado grande.");
                return;
            }

            List<(int IdMaterial, decimal Cantidad)>? materiales =
                esMateriales
                    ? _materiales
                        .Select(m => (m.IdMaterial, m.Cantidad))
                        .ToList()
                    : null;

            btnAgregar.IsEnabled = false;

            try
            {
                await _egresosService.RegistrarAsync(
                    tipo,
                    fecha,
                    esMateriales ? string.Empty : concepto,
                    medio,
                    monto,
                    materiales);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MostrarError("El egreso NO se agregó. " + ex.Message);
                btnAgregar.IsEnabled = true;
            }
        }
    }
}