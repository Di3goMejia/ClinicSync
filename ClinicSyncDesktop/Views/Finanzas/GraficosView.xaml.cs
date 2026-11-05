
using ClinicSyncDesktop.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    public partial class GraficosView : UserControl
    {
        private readonly GraficosApiService? _graficosService;

        // FILTRO INICIAL
        private string filtroActual = "Todos";

        // IDENTIFICA LA CONSULTA MÁS RECIENTE
        private int numeroConsulta = 0;

        public GraficosView()
        {
            InitializeComponent();

            ActualizarBotones();
        }

        public GraficosView(GraficosApiService service)
            : this()
        {
            _graficosService = service;
        }

        // CARGAR DATOS DEL GRÁFICO
        private async Task CargarDatosAsync()
        {
            if (_graficosService is null)
                return;

            int consultaActual = ++numeroConsulta;
            string filtroSolicitado = filtroActual;

            try
            {
                var datos =
                    await _graficosService.ObtenerGraficoAsync(
                        filtroSolicitado);

                // IGNORAR RESPUESTAS DE FILTROS ANTERIORES
                if (consultaActual != numeroConsulta)
                    return;

                TxtTotalIngresos.Text =
                    datos.TotalIngresos.ToString("C2");

                TxtTotalEgresos.Text =
                    datos.TotalEgresos.ToString("C2");

                TxtSaldoNeto.Text =
                    datos.SaldoNeto.ToString("C2");

                // COLOR DEL SALDO
                TxtSaldoNeto.Foreground =
                    datos.SaldoNeto < 0
                        ? new SolidColorBrush(Colors.DarkRed)
                        : new SolidColorBrush(
                            Color.FromRgb(23, 70, 90));

                if (datos.Grafico is null)
                {
                    ChartIngresosEgresos.Series =
                        Array.Empty<ISeries>();

                    return;
                }

                // SERIES DEL GRÁFICO
                ChartIngresosEgresos.Series =
                [
                    new ColumnSeries<decimal>
                    {
                        Name = "Ingresos",
                        Values = datos.Grafico.Ingresos,
                        Fill = new SolidColorPaint(
                            SKColors.ForestGreen)
                    },

                    new ColumnSeries<decimal>
                    {
                        Name = "Egresos",
                        Values = datos.Grafico.Egresos,
                        Fill = new SolidColorPaint(
                            SKColors.Crimson)
                    }
                ];

                // EJE HORIZONTAL
                ChartIngresosEgresos.XAxes =
                [
                    new Axis
                    {
                        Labels = [.. datos.Grafico.Etiquetas],
                        LabelsPaint = new SolidColorPaint(
                            SKColors.DarkSlateGray),
                        TextSize = 14
                    }
                ];

                // EJE VERTICAL
                ChartIngresosEgresos.YAxes =
                [
                    new Axis
                    {
                        Labeler = value => value.ToString("C0"),
                        LabelsPaint = new SolidColorPaint(
                            SKColors.DarkSlateGray)
                    }
                ];
            }
            catch (Exception ex)
            {
                if (consultaActual != numeroConsulta)
                    return;

                MessageBox.Show(
                    "No se pudieron cargar los gráficos: " +
                    ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // CARGAR AL ABRIR LA VISTA
        private async void GraficosView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarDatosAsync();
        }

        // CAMBIAR FILTRO
        private async void CambiarFiltro(string filtro)
        {
            filtroActual = filtro;

            ActualizarBotones();

            await CargarDatosAsync();
        }

        // ACTUALIZAR COLOR DE LOS BOTONES
        private void ActualizarBotones()
        {
            var colorSeleccion = new SolidColorBrush(
                Color.FromRgb(124, 194, 207));

            var normal = Brushes.WhiteSmoke;

            BtnHoy.Background =
                filtroActual == "Hoy"
                    ? colorSeleccion
                    : normal;

            BtnSemana.Background =
                filtroActual == "Semana"
                    ? colorSeleccion
                    : normal;

            BtnMes.Background =
                filtroActual == "Mes"
                    ? colorSeleccion
                    : normal;

            BtnTodos.Background =
                filtroActual == "Todos"
                    ? colorSeleccion
                    : normal;
        }

        // BOTÓN HOY
        private void Hoy_Click(
            object sender,
            RoutedEventArgs e)
        {
            CambiarFiltro("Hoy");
        }

        // BOTÓN SEMANA
        private void Semana_Click(
            object sender,
            RoutedEventArgs e)
        {
            CambiarFiltro("Semana");
        }

        // BOTÓN MES
        private void Mes_Click(
            object sender,
            RoutedEventArgs e)
        {
            CambiarFiltro("Mes");
        }

        // BOTÓN TODOS
        private void Todos_Click(
            object sender,
            RoutedEventArgs e)
        {
            CambiarFiltro("Todos");
        }
    }
}
