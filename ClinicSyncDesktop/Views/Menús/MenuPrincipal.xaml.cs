using ClinicSyncDesktop.Data;
using System;
using System.Windows;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Views.Notificaciones;
using System.Threading.Tasks;
using System.Windows.Threading;


namespace ClinicSync.Views
{
    public partial class MenuPrincipal : Window
    {
        public MenuPrincipal()
        {
            InitializeComponent();
            PrepararNotificaciones();

        }


        private readonly NotificacionesApiService
            servicioNotificaciones = new();

        private readonly DispatcherTimer relojNotificaciones =
            new()
            {
                Interval = TimeSpan.FromMinutes(1)
            };

        private bool cargandoNotificaciones;

        private void PrepararNotificaciones()
        {
            relojNotificaciones.Tick += async (_, _) =>
                await CargarNotificacionesAsync();

            Loaded += async (_, _) =>
            {
                relojNotificaciones.Start();
                await CargarNotificacionesAsync();
            };

            Closed += (_, _) =>
                relojNotificaciones.Stop();
        }


        private async Task CargarNotificacionesAsync()
        {
            if (cargandoNotificaciones)
                return;

            cargandoNotificaciones = true;

            try
            {
                // OBTENER NOTIFICACIONES
                var todas =
                    await servicioNotificaciones.ListarAsync();

                // SOLO NOTIFICACIONES VISIBLES
                var visibles = todas
                    .Where(n => n.Vigente && !n.Archivada)
                    .OrderByDescending(n => n.FechaCreacion)
                    .Take(8)
                    .ToList();

                // INDICADOR DE LA CAMPANA
                PuntoPendientes.Visibility =
                    todas.Any(n =>
                        !n.Leida &&
                        n.Vigente &&
                        !n.Archivada)
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                // LIMPIAR EL CONTENIDO ANTERIOR
                ListaAhora.Children.Clear();
                ListaAnteriores.Children.Clear();

                // DISTRIBUIR LAS NOTIFICACIONES
                foreach (var n in visibles)
                {
                    var destino =
                        n.FechaCreacion.Date == DateTime.Today
                            ? ListaAhora
                            : ListaAnteriores;

                    destino.Children.Add(
                        NotificacionesVisuales.CrearTarjeta(
                            n,

                            // MARCAR COMO LEÍDA
                            async () =>
                            {
                                await servicioNotificaciones
                                    .MarcarLeidaAsync(
                                        n.IdNotificacion);

                                // Volver a consultar después
                                // de cambiar el estado.
                                await CargarNotificacionesAsync();
                            },

                            null));
                }

                // MOSTRAR "AHORA" SOLO SI HAY NOTIFICACIONES
                TituloAhora.Visibility =
                    ListaAhora.Children.Count > 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                // MOSTRAR "MÁS TEMPRANO" SOLO SI HAY NOTIFICACIONES
                TituloAnteriores.Visibility =
                    ListaAnteriores.Children.Count > 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                // MENSAJE ÚNICO SI NO HAY NOTIFICACIONES
                MensajeSinNotificaciones.Visibility =
                    visibles.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Error al cargar notificaciones: " +
                    ex.Message);
            }
            finally
            {
                cargandoNotificaciones = false;
            }
        }



        private async void Ventana_Loaded(
            object sender, RoutedEventArgs e)
        {
            try
            {
                PerfilActual perfil =
                    await ClienteApi.ObtenerPerfilAsync();

                NombrePerfil.Text = perfil.NombreUsuario;
                RolPerfil.Text = perfil.Rol;
            }
            catch (Exception)
            {
                NombrePerfil.Text = "Perfil no disponible";
                RolPerfil.Text = "";
            }
        }

        private void BotonModulo_Click(
            object sender, RoutedEventArgs e)
        {
            if (sender is Button boton)
            {
                string modulo =
                    boton.Tag?.ToString() ?? "Pacientes";

                MenuLateral menu = new MenuLateral(modulo);

                Application.Current.MainWindow = menu;
                menu.Show();
                Close();
            }
        }

        private async void Campana_Click(
            object sender, RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;

            PopupNotificaciones.IsOpen =
                !PopupNotificaciones.IsOpen;

            if (PopupNotificaciones.IsOpen)
            {
                await CargarNotificacionesAsync();
            }
        }

        private void PerfilMenu_Click(
            object sender, RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;
            PopupPerfil.IsOpen = !PopupPerfil.IsOpen;
        }

        private async void MarcarLeidas_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                await servicioNotificaciones.MarcarTodasAsync();

                await CargarNotificacionesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void VerTodas_Click(
            object sender, RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;

            MenuLateral menu = new MenuLateral("Notificaciones");

            Application.Current.MainWindow = menu;

            menu.Show();
            Close();
        }

        private void Configuracion_Click(
            object sender, RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;

            MessageBox.Show(
                "El módulo de configuración estará disponible próximamente.",
                "ClinicSync",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async void CerrarSesion_Click(
            object sender, RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;

            try
            {
                await ClienteApi.CerrarSesionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cerrar sesión: " + ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Login login = new Login();

            Application.Current.MainWindow = login;
            login.Show();
            Close();
        }
    }
}
