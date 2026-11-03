using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Controls.Finanzas
{
    public partial class FinanzasGraficosControl : UserControl
    {
        public event EventHandler? ExportarClick;
        public event EventHandler<string>? FiltroCambiado;

        private List<TransaccionFinanciera> _ingresos = new List<TransaccionFinanciera>();
        private List<TransaccionFinanciera> _egresos = new List<TransaccionFinanciera>();
        private string _filtroActual = "Mes";

        public string FiltroActual => _filtroActual;

        public FinanzasGraficosControl()
        {
            InitializeComponent();
            ActualizarEstiloFiltros();
        }

        public void ActualizarDatos(IEnumerable<TransaccionFinanciera> ingresos, IEnumerable<TransaccionFinanciera> egresos)
        {
            _ingresos = ingresos != null ? new List<TransaccionFinanciera>(ingresos) : new List<TransaccionFinanciera>();
            _egresos = egresos != null ? new List<TransaccionFinanciera>(egresos) : new List<TransaccionFinanciera>();
            CalcularMetricas();
            ActualizarGraficoCurvas();
        }

        public void CambiarFiltro(string nuevoFiltro)
        {
            _filtroActual = nuevoFiltro;
            if (PopoverRango != null && nuevoFiltro != "Rango")
            {
                PopoverRango.Visibility = Visibility.Collapsed;
            }
            ActualizarEstiloFiltros();
            CalcularMetricas();
            ActualizarGraficoCurvas();
        }

        private void CalcularMetricas()
        {
            if (TxtTotalIngresos == null || TxtTotalEgresos == null || TxtSaldoNeto == null) return;

            string? desde = TxtFechaDesde?.Text;
            string? hasta = TxtFechaHasta?.Text;

            var ingresosFiltrados = _ingresos.Where(i => i.Activo && FiltroFinanzasHelper.CoincideFiltro(i.Fecha, _filtroActual, desde, hasta));
            var egresosFiltrados = _egresos.Where(e => e.Activo && FiltroFinanzasHelper.CoincideFiltro(e.Fecha, _filtroActual, desde, hasta));

            decimal totalIngresos = ingresosFiltrados.Sum(i => i.Total);
            decimal totalEgresos = egresosFiltrados.Sum(e => e.Total);
            decimal saldoNeto = totalIngresos - totalEgresos;

            TxtTotalIngresos.Text = $"L {totalIngresos:N2}";
            TxtTotalEgresos.Text = $"L {totalEgresos:N2}";
            TxtSaldoNeto.Text = $"L {saldoNeto:N2}";
            TxtSaldoNeto.Foreground = saldoNeto >= 0
                ? new SolidColorBrush(Color.FromRgb(0x20, 0xA3, 0x82))
                : new SolidColorBrush(Color.FromRgb(0xD9, 0x43, 0x5A));
        }

        private void ActualizarGraficoCurvas()
        {
            if (LineaIngresos == null || LineaEgresos == null || ContenedorEjeX == null) return;

            ContenedorEjeX.Children.Clear();

            PointCollection puntosIngresos = new PointCollection();
            PointCollection puntosEgresos = new PointCollection();

            switch (_filtroActual)
            {
                case "Hoy":
                    // 7 puntos para horas del día (08:00 a 20:00)
                    string[] horas = { "08h", "10h", "12h", "14h", "16h", "18h", "20h" };
                    double[] valsIngresoHoy = { 240, 180, 95, 140, 60, 110, 170 };
                    double[] valsEgresoHoy = { 240, 210, 190, 175, 130, 180, 220 };

                    for (int i = 0; i < horas.Length; i++)
                    {
                        double x = 48 + i * 80;
                        puntosIngresos.Add(new Point(x, valsIngresoHoy[i]));
                        puntosEgresos.Add(new Point(x, valsEgresoHoy[i]));

                        var txt = new TextBlock
                        {
                            Text = horas[i],
                            FontSize = 8.5,
                            Foreground = new SolidColorBrush(Color.FromRgb(0x5A, 0x78, 0x8A))
                        };
                        Canvas.SetLeft(txt, x - 6);
                        Canvas.SetTop(txt, 252);
                        ContenedorEjeX.Children.Add(txt);
                    }
                    break;

                case "Semana":
                    // 7 puntos para días de la semana (Lun a Dom)
                    string[] diasSemana = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
                    double[] valsIngresoSemana = { 190, 140, 80, 160, 45, 115, 230 };
                    double[] valsEgresoSemana = { 210, 175, 120, 190, 95, 160, 240 };

                    for (int i = 0; i < diasSemana.Length; i++)
                    {
                        double x = 48 + i * 80;
                        puntosIngresos.Add(new Point(x, valsIngresoSemana[i]));
                        puntosEgresos.Add(new Point(x, valsEgresoSemana[i]));

                        var txt = new TextBlock
                        {
                            Text = diasSemana[i],
                            FontSize = 8.5,
                            Foreground = new SolidColorBrush(Color.FromRgb(0x5A, 0x78, 0x8A))
                        };
                        Canvas.SetLeft(txt, x - 8);
                        Canvas.SetTop(txt, 252);
                        ContenedorEjeX.Children.Add(txt);
                    }
                    break;

                case "Mes":
                case "Rango":
                default:
                    // 24 puntos del mes (Fiel a la captura oficial de diseño)
                    double[] ptsX = { 48, 69, 90, 111, 132, 153, 174, 195, 216, 237, 258, 279, 300, 321, 342, 363, 384, 405, 426, 447, 468, 489, 510, 531 };
                    double[] ptsYIngresos = { 240, 218, 172, 200, 142, 88, 200, 148, 118, 104, 92, 142, 16, 55, 66, 78, 124, 182, 30, 78, 146, 164, 122, 104 };
                    double[] ptsYEgresos = { 178, 90, 240, 204, 156, 168, 136, 84, 210, 126, 110, 42, 54, 138, 126, 184, 162, 116, 236, 232, 162, 44, 198, 170 };

                    for (int i = 0; i < ptsX.Length; i++)
                    {
                        puntosIngresos.Add(new Point(ptsX[i], ptsYIngresos[i]));
                        puntosEgresos.Add(new Point(ptsX[i], ptsYEgresos[i]));

                        var txt = new TextBlock
                        {
                            Text = i.ToString(),
                            FontSize = 8.5,
                            Foreground = new SolidColorBrush(Color.FromRgb(0x5A, 0x78, 0x8A))
                        };
                        Canvas.SetLeft(txt, ptsX[i] - 3);
                        Canvas.SetTop(txt, 252);
                        ContenedorEjeX.Children.Add(txt);
                    }
                    break;
            }

            LineaIngresos.Points = puntosIngresos;
            LineaEgresos.Points = puntosEgresos;
        }

        private void BtnExportar_Click(object sender, MouseButtonEventArgs e)
        {
            ExportarClick?.Invoke(this, EventArgs.Empty);
        }

        private void Filtro_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                if (border == PillHoy)
                {
                    CambiarFiltro("Hoy");
                }
                else if (border == PillSemana)
                {
                    CambiarFiltro("Semana");
                }
                else if (border == PillMes)
                {
                    CambiarFiltro("Mes");
                }
                else if (border == PillRango)
                {
                    _filtroActual = "Rango";
                    PopoverRango.Visibility = PopoverRango.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    ActualizarEstiloFiltros();
                    CalcularMetricas();
                    ActualizarGraficoCurvas();
                }

                FiltroCambiado?.Invoke(this, _filtroActual);
            }
        }

        private void BtnConfirmarRango_Click(object sender, RoutedEventArgs e)
        {
            PopoverRango.Visibility = Visibility.Collapsed;
            CalcularMetricas();
            ActualizarGraficoCurvas();
            FiltroCambiado?.Invoke(this, _filtroActual);
        }

        private void ActualizarEstiloFiltros()
        {
            var brushActivo = new SolidColorBrush(Color.FromRgb(0x1E, 0x7D, 0x8A));
            var brushInactivo = Brushes.White;
            var textBrushInactivo = (SolidColorBrush)FindResource("ColorTexto");

            PillHoy.Background = _filtroActual == "Hoy" ? brushActivo : brushInactivo;
            TxtHoy.Foreground = _filtroActual == "Hoy" ? Brushes.White : textBrushInactivo;
            TxtHoy.FontWeight = _filtroActual == "Hoy" ? FontWeights.Bold : FontWeights.SemiBold;

            PillSemana.Background = _filtroActual == "Semana" ? brushActivo : brushInactivo;
            TxtSemana.Foreground = _filtroActual == "Semana" ? Brushes.White : textBrushInactivo;
            TxtSemana.FontWeight = _filtroActual == "Semana" ? FontWeights.Bold : FontWeights.SemiBold;

            PillMes.Background = _filtroActual == "Mes" ? brushActivo : brushInactivo;
            TxtMes.Foreground = _filtroActual == "Mes" ? Brushes.White : textBrushInactivo;
            TxtMes.FontWeight = _filtroActual == "Mes" ? FontWeights.Bold : FontWeights.SemiBold;

            PillRango.Background = _filtroActual == "Rango" ? brushActivo : brushInactivo;
            TxtRango.Foreground = _filtroActual == "Rango" ? Brushes.White : textBrushInactivo;
            TxtRango.FontWeight = _filtroActual == "Rango" ? FontWeights.Bold : FontWeights.SemiBold;
        }
    }
}
