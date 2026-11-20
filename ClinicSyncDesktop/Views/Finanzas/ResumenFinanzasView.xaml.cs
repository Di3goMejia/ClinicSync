
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace ClinicSyncDesktop.Views
{
    public partial class ResumenFinanzasView : UserControl
    {
        private readonly ResumenFinanzasApiService servicio;

        // FILTRO INICIAL
        private string filtroActual = "Todos";

        private bool cargando;
        private bool recargarPendiente;

        public ResumenFinanzasView()
            : this(new ResumenFinanzasApiService())
        {
        }

        public ResumenFinanzasView(ResumenFinanzasApiService servicio)
        {
            InitializeComponent();

            this.servicio = servicio;

            ActualizarBotones();
        }

        private async void ResumenFinanzasView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarDatosAsync();
        }

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
                        var movimientos =
                            await servicio.ObtenerResumenAsync(
                                filtroSolicitado);

                        if (filtroSolicitado == filtroActual)
                        {
                            DgResumen.ItemsSource = movimientos
                                .OrderByDescending(m => m.Fecha)
                                .ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (filtroSolicitado == filtroActual)
                        {
                            TxtError.Text =
                                "No se pudo cargar el resumen financiero: "
                                + ex.Message;

                            TxtError.Visibility =
                                Visibility.Visible;

                            DgResumen.ItemsSource =
                                Array.Empty<MovimientoResumen>();
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

        private async void CambiarFiltro(string filtro)
        {
            filtroActual = filtro;

            ActualizarBotones();

            await CargarDatosAsync();
        }

        private void ActualizarBotones()
        {
            var seleccionado = new SolidColorBrush(
                Color.FromRgb(124, 194, 207));

            var normal = Brushes.WhiteSmoke;

            BtnHoy.Background =
                filtroActual == "Hoy" ? seleccionado : normal;

            BtnSemana.Background =
                filtroActual == "Semana" ? seleccionado : normal;

            BtnMes.Background =
                filtroActual == "Mes" ? seleccionado : normal;

            BtnTodos.Background =
                filtroActual == "Todos" ? seleccionado : normal;
        }

        private void Hoy_Click(
            object sender,
            RoutedEventArgs e)
        {
            CambiarFiltro("Hoy");
        }

        private void Semana_Click(
            object sender,
            RoutedEventArgs e)
        {
            CambiarFiltro("Semana");
        }

        private void Mes_Click(
            object sender,
            RoutedEventArgs e)
        {
            CambiarFiltro("Mes");
        }

        private void Todos_Click(
            object sender,
            RoutedEventArgs e)
        {
            CambiarFiltro("Todos");
        }

        private void ExportarPdf_Click(
    object sender,
    RoutedEventArgs e)
        {
            try
            {
                // OBTENER LOS MOVIMIENTOS MOSTRADOS EN LA TABLA
                var movimientos = DgResumen.Items
                    .Cast<MovimientoResumen>()
                    .ToList();

                // CONFIGURAR VENTANA PARA GUARDAR
                SaveFileDialog guardarArchivo = new()
                {
                    Title = "Guardar reporte financiero",
                    Filter = "Archivo PDF (*.pdf)|*.pdf",
                    DefaultExt = ".pdf",
                    AddExtension = true,
                    FileName = "Reporte_Financiero_" +
                               DateTime.Now.ToString("yyyyMMdd_HHmm")
                };

                bool? resultado = guardarArchivo.ShowDialog();

                if (resultado != true)
                    return;

                // GENERAR EL DOCUMENTO
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
                    "No se pudo generar el reporte financiero.\n\n" +
                    ex.Message,
                    "ClinicSync - Finanzas",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}
