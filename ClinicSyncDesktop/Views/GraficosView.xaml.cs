using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// Interaction logic for GraficosView.xaml
    /// </summary>
    public partial class GraficosView : UserControl
    {
        private readonly GraficosApiService? _graficosService;
        private string filtroActual = "Hoy";

        public GraficosView()
        {
            InitializeComponent();
        }

        public GraficosView(GraficosApiService service) : this()
        {
            _graficosService = service;
            CambiarFiltro(filtroActual);
        }

        private async void CargarDatos()
        {
            if (_graficosService is null) return;

            var datos = await _graficosService.ObtenerGraficoAsync(filtroActual);

            TxtTotalIngresos.Text = datos.TotalIngresos.ToString("C2");
            TxtTotalEgresos.Text = datos.TotalEgresos.ToString("C2");
            TxtSaldoNeto.Text = datos.SaldoNeto.ToString("C2");

            if (datos.SaldoNeto < 0)
            {
                TxtSaldoNeto.Foreground = new SolidColorBrush(Colors.DarkRed);
            }

            if (datos.Grafico is null) return;

            ChartIngresosEgresos.Series = [
                new ColumnSeries<decimal>
                {
                    Name = "Ingresos",
                    Values = datos.Grafico.Ingresos,
                    Fill = new SolidColorPaint(SKColors.ForestGreen)
                },
                new ColumnSeries<decimal>
                {
                    Name = "Egresos",
                    Values = datos.Grafico.Egresos,
                    Fill = new SolidColorPaint(SKColors.Crimson)
                }
            ];

            ChartIngresosEgresos.XAxes = [
                new Axis
                {
                    Labels = [.. datos.Grafico.Etiquetas],
                    LabelsPaint = new SolidColorPaint(SKColors.DarkSlateGray),
                    TextSize = 14
                }
            ];

            ChartIngresosEgresos.YAxes =
            [
                new Axis
                {
                    Labeler = value => value.ToString("C0"),
                    LabelsPaint = new SolidColorPaint(SKColors.DarkSlateGray)
                }
            ];
        }

        private void GraficosView_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CambiarFiltro(string filtro)
        {
            filtroActual = filtro;
            ActualizarBotones();
            CargarDatos();
        }

        private void ActualizarBotones()
        {
            var colorSeleccion = Color.FromRgb(124, 194, 207);

            BtnHoy.Background =
                filtroActual == "Hoy"
                    ? new SolidColorBrush(colorSeleccion)
                    : Brushes.WhiteSmoke;
             
            BtnSemana.Background =
                filtroActual == "Semana"
                    ? new SolidColorBrush(colorSeleccion)
                    : Brushes.WhiteSmoke;

            BtnMes.Background =
                filtroActual == "Mes"
                    ? new SolidColorBrush(colorSeleccion)
                    : Brushes.WhiteSmoke;

            BtnTodos.Background =
                filtroActual == "Todos"
                    ? new SolidColorBrush(colorSeleccion)
                    : Brushes.WhiteSmoke;
        }

        private void Hoy_Click(
            object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Hoy");
        }

        private void Semana_Click(
            object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Semana");
        }

        private void Mes_Click(
            object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Mes");
        }

        private void Todos_Click(
            object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Todos");
        }
    }
}
