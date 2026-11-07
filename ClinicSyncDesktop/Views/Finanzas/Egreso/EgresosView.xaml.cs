
using ClinicSyncDesktop.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    public partial class EgresosView : UserControl
    {
        private readonly EgresosApiService? _egresosService;

        // FILTRO INICIAL
        private string _filtro = "Todos";

        private bool cargando;
        private bool recargarPendiente;

        public EgresosView()
        {
            InitializeComponent();
        }

        public EgresosView(EgresosApiService egresosService)
            : this()
        {
            _egresosService = egresosService;

            Loaded += EgresosView_Loaded;
        }

        // MOSTRAR AVISOS
        private void MostrarAviso(
            string mensaje,
            bool exitoso)
        {
            PanelAvisos.Children.Clear();

            AvisoNotificacion aviso = new AvisoNotificacion(
                mensaje,
                exitoso);

            PanelAvisos.Children.Add(aviso);
        }

        // CARGAR EGRESOS
        private async void EgresosView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarEgresosAsync();
        }

        // RANGO DE FECHAS SEGÚN EL FILTRO
        private (DateTime desde, DateTime hasta)
            ObtenerRango()
        {
            DateTime hoy = DateTime.Today;

            return _filtro switch
            {
                "Todos" => (
                    DateTime.MinValue,
                    DateTime.MaxValue),

                "Semana" => (
                    hoy.AddDays(-6),
                    hoy),

                "Mes" => (
                    new DateTime(
                        hoy.Year,
                        hoy.Month,
                        1),

                    new DateTime(
                        hoy.Year,
                        hoy.Month,
                        1).AddMonths(1).AddDays(-1)),

                _ => (hoy, hoy)
            };
        }

        // CONSULTAR EGRESOS
        private async Task CargarEgresosAsync()
        {
            if (_egresosService is null)
            {
                MostrarAviso(
                    "La conexión con la API todavía no está configurada.",
                    false);

                return;
            }

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

                    string filtroSolicitado = _filtro;

                    var (desde, hasta) = ObtenerRango();

                    try
                    {
                        var egresos =
                            await _egresosService.ListarAsync(
                                desde,
                                hasta);

                        if (filtroSolicitado == _filtro)
                        {
                            dgEgresos.ItemsSource = egresos;
                        }
                    }
                    catch (Exception)
                    {
                        if (filtroSolicitado == _filtro)
                        {
                            MostrarAviso(
                                "No fue posible cargar los egresos. " +
                                "Verifica el servidor y tus permisos.",
                                false);
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

        // CAMBIAR FILTRO
        private async void Filtro_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is RadioButton boton &&
                boton.Tag is string filtro)
            {
                _filtro = filtro;

                await CargarEgresosAsync();
            }
        }

        // AGREGAR EGRESO
        private async void btnAgregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_egresosService is null)
            {
                MostrarAviso(
                    "La conexión con la API todavía no está configurada.",
                    false);

                return;
            }

            var ventana = new AgregarEgresoWindow(
                _egresosService)
            {
                Owner = Window.GetWindow(this)
            };

            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                await CargarEgresosAsync();

                MostrarAviso(
                    "El egreso se agregó con éxito.",
                    true);
            }
        }

        // EDITAR EGRESO
        private async void EditarEgreso_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_egresosService is null ||
                sender is not Button boton ||
                boton.DataContext is not
                    ClinicSyncDesktop.Models.Egreso egreso)
            {
                return;
            }

            var ventana = new EditarEgresoWindow(
                egreso.IdEgreso,
                _egresosService)
            {
                Owner = Window.GetWindow(this)
            };

            if (ventana.ShowDialog() == true)
            {
                await CargarEgresosAsync();

                MostrarAviso(
                    "El egreso se actualizó con éxito.",
                    true);
            }
        }
    }
}
