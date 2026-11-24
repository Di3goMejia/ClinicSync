using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class AgregarEgresoWindow : Window
    {
        /// <summary>
        /// The egresos service
        /// </summary>
        private readonly EgresosApiService _egresosService;
        /// <summary>
        /// The cultura
        /// </summary>
        private static readonly CultureInfo Cultura = CultureInfo.InvariantCulture;
        /// <summary>
        /// The formateando
        /// </summary>
        private bool _formateando;
        /// <summary>
        /// The materiales
        /// </summary>
        private List<MaterialParaEgreso> _materiales = new();
        /// <summary>
        /// The validador
        /// </summary>
        private readonly ValidadorEgreso _validador = new();
        /// <summary>
        /// The controles
        /// </summary>
        private readonly Dictionary<string, Control> _controles;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgregarEgresoWindow"/> class.
        /// </summary>
        /// <param name="egresosService">The egresos service.</param>
        public AgregarEgresoWindow(EgresosApiService egresosService)
        {
            InitializeComponent();

            _controles = new Dictionary<string, Control>
            {
                [nameof(EntradaEgreso.Tipo)] = cmbTipo,
                [nameof(EntradaEgreso.Medio)] = cmbMedio,
                [nameof(EntradaEgreso.Concepto)] = txtConcepto,
                [nameof(EntradaEgreso.TextoFecha)] = dpFecha,
                [nameof(EntradaEgreso.TextoMonto)] = txtMonto
            };

            _egresosService = egresosService;

            // FECHA DE HOY POR DEFECTO
            dpFecha.SelectedDate = DateTime.Today;
            txtFecha.Text = DateTime.Today.ToString("yyyy/MM/dd", Cultura);
            txtHora.Text = DateTime.Now.ToString("HH:mm", Cultura);
        }

        // MOVER LA VENTANA DESDE EL ENCABEZADO
        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Encabezado control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Encabezado_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // MENSAJES DE VALIDACIÓN
        /// <summary>
        /// Mostrars the error.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            iconoError.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Ocultars the error.
        /// </summary>
        private void OcultarError()
        {
            txtError.Text = string.Empty;
            iconoError.Visibility = Visibility.Collapsed;
        }

        // ¿HAY DATOS ESCRITOS? (SIN CONTAR LA FECHA POR DEFECTO)
        /// <summary>
        /// Hays the datos escritos.
        /// </summary>
        /// <returns></returns>
        private bool HayDatosEscritos() =>
            cmbTipo.SelectedIndex > 0 ||
            cmbMedio.SelectedIndex > 0 ||
            _materiales.Count > 0 ||
            !string.IsNullOrWhiteSpace(txtConcepto.Text) ||
            !string.IsNullOrWhiteSpace(txtMonto.Text);

        // CERRAR, PIDIENDO CONFIRMACIÓN SI HAY DATOS
        /// <summary>
        /// Handles the Click event of the Cerrar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Eses the materiales.
        /// </summary>
        /// <returns></returns>
        private bool EsMateriales() =>
            ((cmbTipo.SelectedItem as ComboBoxItem)?.Tag as string)
                == "MATERIALES";

        // MATERIALES MUESTRA EL SELECTOR; SERVICIOS, EL TEXTO LIBRE
        /// <summary>
        /// Handles the SelectionChanged event of the cmbTipo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the Materiales control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Actualizars the texto materiales.
        /// </summary>
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

        /// <summary>
        /// Tries the obtener fecha.
        /// </summary>
        /// <param name="fecha">The fecha.</param>
        /// <returns></returns>
        private bool TryObtenerFecha(out DateTime fecha) =>
            DateTime.TryParseExact(
                txtFecha.Text.Trim(),
                "yyyy/MM/dd",
                Cultura,
                DateTimeStyles.None,
                out fecha);

        /// <summary>
        /// Tries the obtener fecha hora.
        /// </summary>
        /// <param name="fecha">The fecha.</param>
        /// <returns></returns>
        private bool TryObtenerFechaHora(out DateTime fecha)
        {
            fecha = default;

            if (!TryObtenerFecha(out DateTime fechaSeleccionada) ||
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

        // ESCRIBE SOLO DÍGITOS Y PONE LAS DIAGONALES SOLAS
        /// <summary>
        /// Handles the TextChanged event of the txtFecha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the LostFocus event of the txtFecha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the Calendario control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Calendario_Click(
            object sender,
            RoutedEventArgs e)
        {
            dpFecha.IsDropDownOpen = true;
        }

        /// <summary>
        /// Dps the fecha selected date changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the PreviewTextInput event of the txtMonto control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextCompositionEventArgs"/> instance containing the event data.</param>
        private void txtMonto_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            e.Handled =
                !Regex.IsMatch(e.Text, @"^[0-9.]+$") ||
                (e.Text.Contains('.') && txtMonto.Text.Contains('.'));
        }

        // ===================== AGREGAR =====================

        /// <summary>
        /// Handles the Click event of the Agregar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

            var resultado = _validador.Validar(new EntradaEgreso(tipo, medio, concepto,
                textoFecha, dpFecha.SelectedDate, txtHora.Text, textoMonto));
            if (!HelperValidacion.AplicarResultado(resultado, _controles, MostrarError, OcultarError))
                return;
            if (esMateriales && _materiales.Count == 0)
            {
                MostrarError("Debe seleccionar al menos un material.");
                return;
            }
            DateTime fecha = resultado.Fecha;
            decimal monto = resultado.Monto;

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