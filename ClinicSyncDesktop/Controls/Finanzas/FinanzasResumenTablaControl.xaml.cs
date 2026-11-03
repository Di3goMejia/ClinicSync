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
    public partial class FinanzasResumenTablaControl : UserControl
    {
        private List<TransaccionFinanciera> _todos = new List<TransaccionFinanciera>();
        private string _filtroActual = "Hoy";

        public event EventHandler<TransaccionFinanciera>? EditarClick;
        public event EventHandler<string>? FiltroCambiado;

        public string FiltroActual => _filtroActual;

        public FinanzasResumenTablaControl()
        {
            InitializeComponent();
        }

        public void CargarDatos(IEnumerable<TransaccionFinanciera> items)
        {
            _todos = items != null ? new List<TransaccionFinanciera>(items) : new List<TransaccionFinanciera>();
            AplicarFiltro();
        }

        public void Refrescar(IEnumerable<TransaccionFinanciera> items)
        {
            _todos = items != null ? new List<TransaccionFinanciera>(items) : new List<TransaccionFinanciera>();
            AplicarFiltro();
        }

        public void CambiarFiltro(string nuevoFiltro)
        {
            _filtroActual = nuevoFiltro;
            if (PopoverRango != null && nuevoFiltro != "Rango")
            {
                PopoverRango.Visibility = Visibility.Collapsed;
            }
            ActualizarEstiloFiltros();
            AplicarFiltro();
        }

        public void AplicarFiltro()
        {
            if (ListaResumen == null) return;

            string? desde = TxtFechaDesde?.Text;
            string? hasta = TxtFechaHasta?.Text;

            var filtrados = _todos.Where(t => FiltroFinanzasHelper.CoincideFiltro(t.Fecha, _filtroActual, desde, hasta)).ToList();

            ListaResumen.ItemsSource = null;
            ListaResumen.ItemsSource = filtrados;
        }

        private void Fila_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement elem && elem.DataContext is TransaccionFinanciera item)
            {
                EditarClick?.Invoke(this, item);
            }
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
                    AplicarFiltro();
                }

                FiltroCambiado?.Invoke(this, _filtroActual);
            }
        }

        private void BtnConfirmarRango_Click(object sender, RoutedEventArgs e)
        {
            PopoverRango.Visibility = Visibility.Collapsed;
            AplicarFiltro();
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
