using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;

namespace ClinicSyncDesktop.Views
{
    public partial class AgendaView : UserControl
    {
        private readonly AgendaApiService agendaService = new();

        // Cantidad de próximas citas que se muestran (Ver más la aumenta).
        private int cantidadProximas = 4;

        public AgendaView()
        {
            InitializeComponent();
            Loaded += AgendaView_Loaded;
        }

        private async void AgendaView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -= AgendaView_Loaded;
            await CargarProximasAsync();
        }

        // CARGAR PRÓXIMAS CITAS
        private async Task CargarProximasAsync()
        {
            try
            {
                List<ProximaCita> citas =
                    await agendaService.ProximasAsync(cantidadProximas);

                listaProximas.ItemsSource = citas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible cargar las próximas citas.\n\n" + ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // + AGREGAR NUEVA CITA
        private async void btnAgregarCita_Click(
            object sender,
            RoutedEventArgs e)
        {
            var ventana = new AgregarCitaWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (ventana.ShowDialog() == true)
            {
                await CargarProximasAsync();
                // Más adelante también recargaremos el calendario.
            }
        }

        // VER MÁS...
        private async void btnVerMas_Click(
            object sender,
            RoutedEventArgs e)
        {
            cantidadProximas += 4;
            await CargarProximasAsync();
        }
    }
}