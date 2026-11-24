using ClinicSyncDesktop.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class EgresosView : UserControl
    {
        /// <summary>
        /// The egresos service
        /// </summary>
        private readonly EgresosApiService? _egresosService;

        // FILTRO INICIAL
        /// <summary>
        /// The filtro
        /// </summary>
        private string _filtro = "Todos";
        /// <summary>
        /// The estado
        /// </summary>
        private string _estado = "Todos";
        /// <summary>
        /// The cargando
        /// </summary>
        private bool cargando;
        /// <summary>
        /// The recargar pendiente
        /// </summary>
        private bool recargarPendiente;

        /// <summary>
        /// Initializes a new instance of the <see cref="EgresosView"/> class.
        /// </summary>
        public EgresosView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EgresosView"/> class.
        /// </summary>
        /// <param name="egresosService">The egresos service.</param>
        public EgresosView(EgresosApiService egresosService)
            : this()
        {
            _egresosService = egresosService;

            Loaded += EgresosView_Loaded;
        }

        // MOSTRAR AVISOS
        /// <summary>
        /// Mostrars the aviso.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="exitoso">if set to <c>true</c> [exitoso].</param>
        private void MostrarAviso(
            string mensaje,
            bool exitoso)
        {
            // Evitar el error si el panel no está disponible.
            if (PanelAvisos == null)
            {
                System.Diagnostics.Debug.WriteLine(mensaje);
                return;
            }

            PanelAvisos.Children.Clear();

            AvisoNotificacion aviso =
                new AvisoNotificacion(mensaje, exitoso);

            PanelAvisos.Children.Add(aviso);
        }


        // CARGAR EGRESOS
        /// <summary>
        /// Handles the Loaded event of the EgresosView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void EgresosView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarEgresosAsync();
        }

        // RANGO DE FECHAS SEGÚN EL FILTRO

        /// <summary>
        /// Obteners the rango.
        /// </summary>
        /// <returns></returns>
        private (DateTime desde, DateTime hasta) ObtenerRango()
        {
            DateTime hoy = DateTime.Today;

            return _filtro switch
            {
                "Todos" => (
                    DateTime.MinValue,
                    hoy),

                "Semana" => (
                    hoy.AddDays(-6),
                    hoy),

                "Mes" => (
                    new DateTime(hoy.Year, hoy.Month, 1),
                    hoy),

                _ => (hoy, hoy)
            };
        }


        // CONSULTAR EGRESOS
        /// <summary>
        /// Cargars the egresos asynchronous.
        /// </summary>
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
                    string estadoSolicitado = _estado;
                    var (desde, hasta) = ObtenerRango();
                    bool? activo = _estado switch
                    {
                        "Activos" => true,
                        "Inactivos" => false,
                        _ => null
                    };


                    try
                    {
                        var egresos = await _egresosService.ListarAsync(
                            desde,
                            hasta,
                            activo);
                        if (filtroSolicitado == _filtro &&
                            estadoSolicitado == _estado)
                        {
                            var listaFiltrada = activo.HasValue
                                ? egresos.Where(x => x.Activo == activo.Value).ToList()
                                : egresos;

                            dgEgresos.ItemsSource = listaFiltrada;
                        }

                    }

                    catch (Exception ex)
                    {
                        if (filtroSolicitado == _filtro &&
                            estadoSolicitado == _estado)
                        {
                            MostrarAviso(
                                "No fue posible cargar los egresos: " +
                                ex.Message,
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
        /// <summary>
        /// Handles the Click event of the Filtro control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the Click event of the btnAgregar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the Click event of the EditarEgreso control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the SelectionChanged event of the CmbEstadoEgresos control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private async void CmbEstadoEgresos_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (CmbEstadoEgresos == null)
                return;

            _estado =
                (CmbEstadoEgresos.SelectedItem as ComboBoxItem)?
                .Tag?.ToString() ?? "Todos";

            await CargarEgresosAsync();
        }

    }
}
