using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    public partial class ResumenFinanzasView : UserControl
    {
        private readonly ResumenFinanzasApiService servicio;
        private string filtroActual = "Hoy";
        private bool cargando;

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

        private async void ResumenFinanzasView_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            if (cargando)
                return;

            cargando = true;
            TxtError.Visibility = Visibility.Collapsed;

            try
            {
                var movimientos = await servicio.ObtenerResumenAsync(filtroActual);
                DgResumen.ItemsSource = movimientos
                    .OrderByDescending(m => m.Fecha)
                    .ToList();
            }
            catch (Exception ex)
            {
                TxtError.Text = "No se pudo cargar el resumen financiero: " + ex.Message;
                TxtError.Visibility = Visibility.Visible;
                DgResumen.ItemsSource = Array.Empty<MovimientoResumen>();
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
            var seleccionado = new SolidColorBrush(Color.FromRgb(124, 194, 207));
            var normal = Brushes.WhiteSmoke;

            BtnHoy.Background = filtroActual == "Hoy" ? seleccionado : normal;
            BtnSemana.Background = filtroActual == "Semana" ? seleccionado : normal;
            BtnMes.Background = filtroActual == "Mes" ? seleccionado : normal;
            BtnTodos.Background = filtroActual == "Todos" ? seleccionado : normal;
        }

        private async void Hoy_Click(object sender, RoutedEventArgs e) => CambiarFiltro("Hoy");
        private async void Semana_Click(object sender, RoutedEventArgs e) => CambiarFiltro("Semana");
        private async void Mes_Click(object sender, RoutedEventArgs e) => CambiarFiltro("Mes");
        private async void Todos_Click(object sender, RoutedEventArgs e) => CambiarFiltro("Todos");
    }
}
