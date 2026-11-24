using System.Linq;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using System;
using System.Globalization;
using System.Windows.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class EditarEgresoWindow : Window
    {
        /// <summary>
        /// The identifier egreso
        /// </summary>
        private readonly int _idEgreso;
        /// <summary>
        /// The servicio
        /// </summary>
        private readonly EgresosApiService _servicio;

        /// <summary>
        /// The cultura
        /// </summary>
        private static readonly CultureInfo Cultura =
            CultureInfo.InvariantCulture;

        /// <summary>
        /// The guardando
        /// </summary>
        private bool _guardando;
        /// <summary>
        /// The materiales
        /// </summary>
        private List<MaterialParaEgreso>? _materiales;
        /// <summary>
        /// The materiales modificados
        /// </summary>
        private bool _materialesModificados;
        /// <summary>
        /// The tipo original
        /// </summary>
        private string _tipoOriginal = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="EditarEgresoWindow"/> class.
        /// </summary>
        /// <param name="idEgreso">The identifier egreso.</param>
        /// <param name="servicio">The servicio.</param>
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
        /// <summary>
        /// Handles the Loaded event of the EditarEgresoWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
                dpFecha.SelectedDate = egreso.Fecha.Date;
                txtFecha.Text = egreso.Fecha.ToString("yyyy/MM/dd", Cultura);
                _tipoOriginal = egreso.Tipo.Trim().ToUpperInvariant();
                txtTipo.Text = _tipoOriginal switch
                {
                    "MATERIALES" => "Materiales",
                    "SERVICIOS" => "Servicios",
                    _ => egreso.Tipo
                };

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
                        item.Tag as string ?? item.Content?.ToString(),
                        egreso.MedioPago,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        cmbMedio.SelectedItem = item;
                        break;
                    }
                }

                // ESTADO ACTUAL DEL EGRESO
                chkActivo.IsChecked = egreso.Activo;

                if (_tipoOriginal == "MATERIALES")
                {
                    _materiales = egreso.Materiales;
                    MostrarMateriales();
                    panelMaterialesEdicion.Visibility = Visibility.Visible;
                }

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

        /// <summary>
        /// Mostrars the materiales.
        /// </summary>
        private void MostrarMateriales()
        {
            txtMaterialesEdicion.Text = "Materiales: " + string.Join(", ",
                _materiales!.Select(m => $"{m.Nombre} ({m.Cantidad:0.##})"));
            txtMaterialesEdicion.ToolTip = txtMaterialesEdicion.Text;
        }

        /// <summary>
        /// Handles the Click event of the EditarMateriales control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void EditarMateriales_Click(object sender, RoutedEventArgs e)
        {
            if (_guardando || _materiales == null)
                return;

            var selector = new SeleccionarMaterialesWindow(_materiales) { Owner = this };
            if (selector.ShowDialog() != true)
                return;

            if (selector.Seleccionados.Count == 0)
            {
                txtError.Text = "Seleccioná al menos un material para este egreso.";
                return;
            }

            _materiales = selector.Seleccionados;
            _materialesModificados = true;
            string concepto = "Compra: " + string.Join(", ", _materiales
                .OrderBy(m => m.Nombre, StringComparer.CurrentCulture)
                .ThenBy(m => m.IdMaterial)
                .Select(m => $"{m.Nombre} ({m.Cantidad.ToString("0.##", Cultura)})"));
            txtConcepto.Text = concepto.Length <= 255 ? concepto : concepto[..255];
            txtError.Text = "";
            MostrarMateriales();
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Encabezado control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// Handles the Click event of the Calendario control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Calendario_Click(object sender, RoutedEventArgs e)
        {
            dpFecha.IsDropDownOpen = true;
        }

        /// <summary>
        /// Handles the TextChanged event of the txtFecha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void txtFecha_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox campo && txtPlaceholderFecha != null)
                txtPlaceholderFecha.Visibility = string.IsNullOrWhiteSpace(campo.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the LostFocus event of the txtFecha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void txtFecha_LostFocus(object sender, RoutedEventArgs e)
        {
            if (LeerFecha(out DateTime fecha) && dpFecha.SelectedDate != fecha)
                dpFecha.SelectedDate = fecha;
        }

        /// <summary>
        /// Handles the SelectedDateChanged event of the dpFecha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void dpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtFecha != null && dpFecha.SelectedDate is DateTime fecha)
                txtFecha.Text = fecha.ToString("yyyy/MM/dd", Cultura);
        }

        /// <summary>
        /// Leers the fecha.
        /// </summary>
        /// <param name="fecha">The fecha.</param>
        /// <returns></returns>
        private bool LeerFecha(out DateTime fecha)
        {
            return DateTime.TryParseExact(txtFecha.Text.Trim(), "yyyy/MM/dd",
                Cultura, DateTimeStyles.None, out fecha);
        }

        // OBTENER FECHA Y HORA
        /// <summary>
        /// Tries the obtener fecha hora.
        /// </summary>
        /// <param name="fecha">The fecha.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handles the Click event of the Guardar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Guardar_Click(
            object sender, RoutedEventArgs e)
        {
            if (_guardando)
                return;

            txtError.Text = "";

            if (!LeerFecha(out DateTime fechaEscrita))
            {
                txtError.Text = "Ingrese una fecha válida en formato AAAA/MM/DD.";
                return;
            }

            dpFecha.SelectedDate = fechaEscrita;
            var medioSeleccionado = cmbMedio.SelectedItem as ComboBoxItem;
            var resultadoValidacion = new ValidadorEgreso().Validar(new EntradaEgreso(
                _tipoOriginal, medioSeleccionado?.Tag as string ?? medioSeleccionado?.Content?.ToString(),
                txtConcepto.Text.Trim(), dpFecha.SelectedDate?.ToString("yyyy/MM/dd") ?? "", dpFecha.SelectedDate,
                txtHora.Text.Trim(), txtMonto.Text.Trim()));
            if (!HelperValidacion.AplicarResultado(resultadoValidacion,
                new Dictionary<string, Control>
                {
                    [nameof(EntradaEgreso.Medio)] = cmbMedio,
                    [nameof(EntradaEgreso.Concepto)] = txtConcepto,
                    [nameof(EntradaEgreso.TextoFecha)] = txtFecha,
                    [nameof(EntradaEgreso.TextoMonto)] = txtMonto
                }, mensaje => txtError.Text = mensaje, () => txtError.Text = ""))
                return;

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
                    medio.Tag as string ?? medio.Content?.ToString() ?? "",
                    monto,
                    activo,
                    materiales: _materialesModificados
                        ? _materiales!.Select(m => (m.IdMaterial, m.Cantidad))
                        : null);

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
        /// <summary>
        /// Handles the Click event of the Cancelar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cancelar_Click(
            object sender, RoutedEventArgs e)
        {
            if (_guardando)
                return;

            DialogResult = false;
        }
    }
}
