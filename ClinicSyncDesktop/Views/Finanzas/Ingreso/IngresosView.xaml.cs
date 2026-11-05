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
    public partial class IngresosView : UserControl
    {
        // SERVICIO DE INGRESOS
        private readonly IngresosApiService servicio = new();

        // LISTA COMPLETA DE INGRESOS
        private IngresoListado[] ingresos =
            Array.Empty<IngresoListado>();

        // FILTRO SELECCIONADO
        private string filtroActual = "Todos";

        // EVENTO PARA EDICIÓN
        public event Action<IngresoListado>? EditarIngresoSolicitado;

        // EVITA CARGAS SIMULTÁNEAS
        private bool cargandoIngresos = false;

        // CONSTRUCTOR
        public IngresosView()
        {
            InitializeComponent();

            // Al abrir, seleccionar Todos.
            filtroActual = "Todos";

            // Actualizar colores de los botones.
            ActualizarBotones();

            // Cargar ingresos cuando aparezca la pantalla.
            Loaded += IngresosView_Loaded;
        }

        // CARGAR INGRESOS AL ABRIR LA PANTALLA
        private async void IngresosView_Loaded(
            object sender, RoutedEventArgs e)
        {
            await CargarIngresosAsync();
        }

        // OBTENER INGRESOS DESDE LA API
        private async Task CargarIngresosAsync()
        {
            if (cargandoIngresos)
                return;

            cargandoIngresos = true;

            ErrorListado.Text = "";
            ErrorListado.Visibility = Visibility.Collapsed;

            try
            {
                var resultado =
                    await servicio.ObtenerIngresosAsync();

                ingresos = resultado ??
                    Array.Empty<IngresoListado>();

                // Aplicar el filtro activo a los nuevos datos.
                AplicarFiltro();

                // Actualizar la tabla.
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

        // MOSTRAR ERRORES EN PANTALLA
        private void MostrarError(string mensaje)
        {
            ErrorListado.Text = mensaje;
            ErrorListado.Visibility = Visibility.Visible;
        }

        // FILTRAR LOS INGRESOS SEGÚN LA FECHA
        private void AplicarFiltro()
        {
            DateTime hoy = DateTime.Today;

            var consulta = ingresos.AsEnumerable();

            switch (filtroActual)
            {
                case "Todos":
                    // Mostrar todos sin filtrar por fecha.
                    break;

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
                    break;
            }

            DgIngresos.ItemsSource = consulta
                .OrderByDescending(i => i.Fecha)
                .ThenByDescending(i => i.IdIngreso)
                .ToList();
        }

        // CAMBIAR FILTRO Y ACTUALIZAR LA TABLA
        private void CambiarFiltro(string filtro)
        {
            filtroActual = filtro;

            // Oscurecer el botón seleccionado.
            ActualizarBotones();

            // Mostrar los ingresos correspondientes.
            AplicarFiltro();
        }

        // ACTUALIZAR LOS COLORES DE LOS CUATRO BOTONES
        private void ActualizarBotones()
        {
            // Color del botón seleccionado.
            Brush colorSeleccionado =
                new SolidColorBrush(
                    Color.FromRgb(124, 194, 207));

            // Color de los botones no seleccionados.
            Brush colorNormal = Brushes.WhiteSmoke;

            BtnHoy.Background =
                filtroActual == "Hoy"
                    ? colorSeleccionado
                    : colorNormal;

            BtnSemana.Background =
                filtroActual == "Semana"
                    ? colorSeleccionado
                    : colorNormal;

            BtnMes.Background =
                filtroActual == "Mes"
                    ? colorSeleccionado
                    : colorNormal;

            BtnTodos.Background =
                filtroActual == "Todos"
                    ? colorSeleccionado
                    : colorNormal;
        }

        // BOTÓN HOY
        private void Hoy_Click(
            object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Hoy");
        }

        // BOTÓN SEMANA
        private void Semana_Click(
            object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Semana");
        }

        // BOTÓN MES
        private void Mes_Click(
            object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Mes");
        }

        // BOTÓN TODOS
        private void BtnTodos_Click(
            object sender, RoutedEventArgs e)
        {
            CambiarFiltro("Todos");
        }

        // AGREGAR NUEVO INGRESO
        private async void AgregarNuevo_Click(
            object sender, RoutedEventArgs e)
        {
            var ventana = new AgregarIngresoWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (ventana.ShowDialog() == true)
            {
                // Recargar la tabla después de guardar.
                await CargarIngresosAsync();
            }
        }

        // EDITAR INGRESO CON EL BOTÓN ENGRANAJE
        private async void EditarIngreso_Click(
            object sender, RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.Tag is not IngresoListado ingreso)
            {
                return;
            }

            // Abrir la ventana de edición.
            var ventana = new EditarIngresoWindow(
                ingreso.IdIngreso)
            {
                Owner = Window.GetWindow(this)
            };

            // Recargar cuando se guarden los cambios.
            if (ventana.ShowDialog() == true)
            {
                await CargarIngresosAsync();
            }
        }
    }
}


