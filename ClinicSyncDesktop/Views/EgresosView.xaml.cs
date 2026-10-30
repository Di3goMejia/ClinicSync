using ClinicSyncDesktop.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
    public partial class EgresosView : UserControl
    {
        private readonly EgresosApiService? _egresosService;
        private string _filtro = "Hoy";

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

        // Mostrar notificaciones con el mismo diseño que Pacientes.
        private void MostrarAviso(string mensaje, bool exitoso)
        {
            PanelAvisos.Children.Clear();

            AvisoPaciente aviso = new AvisoPaciente(
                mensaje,
                exitoso);

            PanelAvisos.Children.Add(aviso);
        }

        private async void EgresosView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarEgresosAsync();
        }

        // RANGO DE FECHAS SEGÚN EL FILTRO ELEGIDO
        private (DateTime desde, DateTime hasta) ObtenerRango()
        {
            DateTime hoy = DateTime.Today;

            return _filtro switch
            {
                "Semana" => (hoy.AddDays(-6), hoy),

                "Mes" => (
                    new DateTime(hoy.Year, hoy.Month, 1),
                    new DateTime(hoy.Year, hoy.Month, 1)
                        .AddMonths(1).AddDays(-1)),

                _ => (hoy, hoy)
            };
        }

        private async Task CargarEgresosAsync()
        {
            if (_egresosService is null)
            {
                MostrarAviso(
                    "La conexión con la API todavía no está configurada.",
                    false);

                return;
            }

            try
            {
                var (desde, hasta) = ObtenerRango();

                dgEgresos.ItemsSource =
                    await _egresosService.ListarAsync(desde, hasta);
            }
            catch (Exception)
            {
                MostrarAviso(
                    "No fue posible cargar los egresos. Verifica el servidor y tus permisos.",
                    false);
            }
        }

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

        private void btnAgregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            // PENDIENTE: abrir AgregarEgresoWindow (siguiente paso).
            MostrarAviso(
                "El formulario de egresos se agregará en el siguiente paso.",
                false);
        }
    }
}