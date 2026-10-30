
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    public partial class IngresosView : UserControl
    {
        private readonly IngresosApiService servicio = new();

        private IngresoListado[] ingresos =
            Array.Empty<IngresoListado>();

        private string filtroActual = "Todos";

        // Permite conectar el engranaje con la edición.
        public event Action<IngresoListado>? EditarIngresoSolicitado;

        private bool cargandoIngresos = false;

        public IngresosView()
        {
            InitializeComponent();

            filtroActual = "Todos";

            Loaded += IngresosView_Loaded;

            ActualizarBotones();
        }

        private async void IngresosView_Loaded(
            object sender, RoutedEventArgs e)
        {
            await CargarIngresosAsync();
        }



        private async Task CargarIngresosAsync()
        {
            if (cargandoIngresos)
                return;

            cargandoIngresos = true;

            ErrorListado.Text = "";
            ErrorListado.Visibility = Visibility.Collapsed;

            try
            {
                var resultado = await servicio.ObtenerIngresosAsync();

                ingresos = resultado ?? Array.Empty<IngresoListado>();

                AplicarFiltro();

                DgIngresos.Items.Refresh();
            }
            catch (Exception ex)
            {
                MostrarError(
                    "No se pudieron cargar los ingresos: " +
                    ex.Message);
            }
            finally
            {
                cargandoIngresos = false;
            }
        }


        private void MostrarError(string mensaje)
        {
            ErrorListado.Text = mensaje;
            ErrorListado.Visibility = Visibility.Visible;
        }



        private void AplicarFiltro()
        {
            DateTime hoy = DateTime.Today;

            var consulta = ingresos.AsEnumerable();

            switch (filtroActual)
            {
                case "Hoy":
                    consulta = consulta.Where(i =>
                        i.Fecha.Date == hoy);
                    break;

                case "Semana":
                    int diasDesdeLunes =
                        ((int)hoy.DayOfWeek + 6) % 7;

                    DateTime inicioSemana =
                        hoy.AddDays(-diasDesdeLunes);

                    consulta = consulta.Where(i =>
                        i.Fecha.Date >= inicioSemana &&
                        i.Fecha.Date <= hoy);
                    break;

                case "Mes":
                    DateTime inicioMes =
                        new DateTime(hoy.Year, hoy.Month, 1);

                    consulta = consulta.Where(i =>
                        i.Fecha.Date >= inicioMes &&
                        i.Fecha.Date <= hoy);
                    break;

                default:
                    // Al entrar, mostrar todos los ingresos.
                    break;
            }

            DgIngresos.ItemsSource = consulta
                .OrderByDescending(i => i.Fecha)
                .ThenByDescending(i => i.IdIngreso)
                .ToList();
        }



        private void CambiarFiltro(string filtro)
        {
            filtroActual = filtro;
            ActualizarBotones();
            AplicarFiltro();
        }

        private void ActualizarBotones()
        {
            BtnHoy.Background =
                filtroActual == "Hoy"
                    ? new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(124, 194, 207))
                    : System.Windows.Media.Brushes.WhiteSmoke;

            BtnSemana.Background =
                filtroActual == "Semana"
                    ? new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(124, 194, 207))
                    : System.Windows.Media.Brushes.WhiteSmoke;

            BtnMes.Background =
                filtroActual == "Mes"
                    ? new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(124, 194, 207))
                    : System.Windows.Media.Brushes.WhiteSmoke;
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

        private async void AgregarNuevo_Click(
            object sender, RoutedEventArgs e)
        {
            var ventana = new AgregarIngresoWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (ventana.ShowDialog() == true)
            {
                await CargarIngresosAsync();
            }
        }



        private async void EditarIngreso_Click(
            object sender, RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.Tag is not IngresoListado ingreso)
                return;

            // Abrir la ventana de edición del ingreso seleccionado
            var ventana = new EditarIngresoWindow(ingreso.IdIngreso)
            {
                Owner = Window.GetWindow(this)
            };

            // Si se guardaron cambios, actualizar el DataGrid
            if (ventana.ShowDialog() == true)
            {
                await CargarIngresosAsync();
            }
        }


    }
}

