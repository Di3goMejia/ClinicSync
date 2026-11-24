using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ResumenFinanzasView : UserControl
    {
        /// <summary>
        /// The servicio
        /// </summary>
        private readonly ResumenFinanzasApiService servicio;
        /// <summary>
        /// The filtro actual
        /// </summary>
        private string filtroActual = "Todos";
        /// <summary>
        /// The cargando
        /// </summary>
        private bool cargando;
        /// <summary>
        /// The recargar pendiente
        /// </summary>
        private bool recargarPendiente;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResumenFinanzasView"/> class.
        /// </summary>
        public ResumenFinanzasView()
            : this(new ResumenFinanzasApiService())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResumenFinanzasView"/> class.
        /// </summary>
        /// <param name="servicio">The servicio.</param>
        public ResumenFinanzasView(ResumenFinanzasApiService servicio)
        {
            InitializeComponent();

            this.servicio = servicio;
            ActualizarBotones();
        }

        /// <summary>
        /// Handles the Loaded event of the ResumenFinanzasView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ResumenFinanzasView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarDatosAsync();
        }

        /// <summary>
        /// Cargars the datos asynchronous.
        /// </summary>
        private async Task CargarDatosAsync()
        {
            if (cargando)
            {
                recargarPendiente = true;
                return;
            }

            cargando = true;

            try
            {
                do
                {
                    recargarPendiente = false;
                    string filtroSolicitado = filtroActual;

                    TxtError.Visibility = Visibility.Collapsed;

                    try
                    {
                        var movimientos = await servicio.ObtenerResumenAsync(
                            filtroSolicitado);

                        if (filtroSolicitado == filtroActual)
                        {
                            DgResumen.ItemsSource = movimientos
                                .OrderByDescending(movimiento => movimiento.Fecha)
                                .ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (filtroSolicitado == filtroActual)
                        {
                            TxtError.Text =
                                "No se pudo cargar el resumen financiero: " + ex.Message;

                            TxtError.Visibility = Visibility.Visible;
                            DgResumen.ItemsSource = Array.Empty<MovimientoResumen>();
                        }
                    }
                }
                while (recargarPendiente);
            }
            finally
            {
                cargando = false;
            }
        }

        /// <summary>
        /// Cambiars the filtro.
        /// </summary>
        /// <param name="filtro">The filtro.</param>
        private async void CambiarFiltro(string filtro)
        {
            filtroActual = filtro;
            ActualizarBotones();
            await CargarDatosAsync();
        }

        /// <summary>
        /// Actualizars the botones.
        /// </summary>
        private void ActualizarBotones()
        {
            var seleccionado = new SolidColorBrush(Color.FromRgb(124, 194, 207));
            var normal = Brushes.WhiteSmoke;

            BtnHoy.Background = filtroActual == "Hoy" ? seleccionado : normal;
            BtnSemana.Background = filtroActual == "Semana" ? seleccionado : normal;
            BtnMes.Background = filtroActual == "Mes" ? seleccionado : normal;
            BtnTodos.Background = filtroActual == "Todos" ? seleccionado : normal;
        }

        /// <summary>
        /// Handles the Click event of the Hoy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Hoy_Click(object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Hoy");
        }

        /// <summary>
        /// Handles the Click event of the Semana control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Semana_Click(object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Semana");
        }

        /// <summary>
        /// Handles the Click event of the Mes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Mes_Click(object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Mes");
        }

        /// <summary>
        /// Handles the Click event of the Todos control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Todos_Click(object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Todos");
        }

        /// <summary>
        /// Handles the Click event of the ExportarPdf control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ExportarPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var movimientos = DgResumen.Items
                    .Cast<MovimientoResumen>()
                    .ToList();

                SaveFileDialog guardarArchivo = new()
                {
                    Title = "Guardar reporte financiero",
                    Filter = "Archivo PDF (*.pdf)|*.pdf",
                    DefaultExt = ".pdf",
                    AddExtension = true,
                    FileName = "Reporte_Financiero_" +
                               DateTime.Now.ToString("yyyyMMdd_HHmm")
                };

                if (guardarArchivo.ShowDialog() != true)
                    return;

                ReporteFinancieroPdfService generador = new();

                generador.GenerarReporte(
                    guardarArchivo.FileName,
                    movimientos,
                    filtroActual);

                PanelAvisos.Visibility = Visibility.Visible;
                PanelAvisos.Children.Add(
                    new AvisoNotificacion(
                        "El reporte financiero se generó correctamente.",
                        true));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo generar el reporte financiero.\n\n" + ex.Message,
                    "ClinicSync - Finanzas",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public sealed class TextoInicialMayusculaConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />, the valid null value is used.
        /// </returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is not string texto || string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            string textoNormalizado = texto.Trim().ToLower(culture);

            return char.ToUpper(textoNormalizado[0], culture)
                + textoNormalizado.Substring(1);
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />, the valid null value is used.
        /// </returns>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}