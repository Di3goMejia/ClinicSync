using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using ClinicSyncDesktop.Views.Notificaciones;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ClinicSync.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MenuPrincipal : Window
    {
        /// <summary>
        /// The servicio notificaciones
        /// </summary>
        private readonly NotificacionesApiService servicioNotificaciones = new();

        /// <summary>
        /// The reloj notificaciones
        /// </summary>
        private readonly DispatcherTimer relojNotificaciones = new()
        {
            Interval = TimeSpan.FromMinutes(1)
        };

        /// <summary>
        /// The cargando notificaciones
        /// </summary>
        private bool cargandoNotificaciones;
        /// <summary>
        /// The es administrador
        /// </summary>
        private bool esAdministrador;

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuPrincipal"/> class.
        /// </summary>
        public MenuPrincipal()
        {
            InitializeComponent();
            PrepararNotificaciones();
        }

        /// <summary>
        /// Preparars the notificaciones.
        /// </summary>
        private void PrepararNotificaciones()
        {
            relojNotificaciones.Tick += async (_, _) =>
                await CargarNotificacionesAsync();

            Loaded += async (_, _) =>
            {
                relojNotificaciones.Start();
                await CargarNotificacionesAsync();
            };

            Closed += (_, _) => relojNotificaciones.Stop();
        }

        /// <summary>
        /// Cargars the notificaciones asynchronous.
        /// </summary>
        private async Task CargarNotificacionesAsync()
        {
            if (cargandoNotificaciones)
                return;

            cargandoNotificaciones = true;

            try
            {
                var todas = await servicioNotificaciones.ListarAsync();

                var visibles = todas
                    .Where(n => n.Vigente && !n.Archivada && !n.Leida)
                    .OrderByDescending(n => n.FechaCreacion)
                    .Take(8)
                    .ToList();

                PuntoPendientes.Visibility = todas.Any(n =>
                    !n.Leida && n.Vigente && !n.Archivada)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                ListaAhora.Children.Clear();
                ListaAnteriores.Children.Clear();

                foreach (var n in visibles)
                {
                    var destino = n.FechaCreacion.Date == DateTime.Today
                        ? ListaAhora
                        : ListaAnteriores;

                    destino.Children.Add(
                        NotificacionesVisuales.CrearTarjeta(
                            n,
                            async () =>
                            {
                                await servicioNotificaciones
                                    .MarcarLeidaAsync(n.IdNotificacion);

                                await CargarNotificacionesAsync();
                            },
                            null));
                }

                TituloAhora.Visibility = ListaAhora.Children.Count > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                TituloAnteriores.Visibility = ListaAnteriores.Children.Count > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                MensajeSinNotificaciones.Visibility = visibles.Count == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Error al cargar notificaciones: " + ex.Message);
            }
            finally
            {
                cargandoNotificaciones = false;
            }
        }

        /// <summary>
        /// Handles the Loaded event of the Ventana control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Ventana_Loaded(object sender, RoutedEventArgs e)
        {
            esAdministrador = false;
            BtnSeguridad.Visibility = Visibility.Collapsed;

            try
            {
                PerfilActual perfil = await ClienteApi.ObtenerPerfilAsync();

                NombrePerfil.Text = perfil.NombreCorto;
                RolPerfil.Text = perfil.Rol;

                esAdministrador = ValidadorRol.EsAdministrador(perfil.Rol);

                BtnSeguridad.Visibility = esAdministrador
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch (Exception)
            {
                NombrePerfil.Text = "Perfil no disponible";
                RolPerfil.Text = "";
            }
        }

        /// <summary>
        /// Handles the Click event of the BotonModulo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BotonModulo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton)
            {
                string modulo = boton.Tag?.ToString() ?? "Pacientes";

                if (modulo == "Seguridad" && !esAdministrador)
                    return;

                MenuLateral menu = new MenuLateral(modulo);

                Application.Current.MainWindow = menu;
                menu.Show();
                Close();
            }
        }

        /// <summary>
        /// Handles the Click event of the Campana control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Campana_Click(object sender, RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;
            PopupNotificaciones.IsOpen = !PopupNotificaciones.IsOpen;

            if (PopupNotificaciones.IsOpen)
            {
                await CargarNotificacionesAsync();
            }
        }

        /// <summary>
        /// Handles the Click event of the PerfilMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PerfilMenu_Click(object sender, RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;
            PopupPerfil.IsOpen = !PopupPerfil.IsOpen;
        }

        /// <summary>
        /// Handles the Click event of the MarcarLeidas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void MarcarLeidas_Click(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Handles the Click event of the VerTodas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void VerTodas_Click(object sender, RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;

            MenuLateral menu = new MenuLateral("Notificaciones");

            Application.Current.MainWindow = menu;
            menu.Show();
            Close();
        }

        /// <summary>
        /// Handles the Click event of the Configuracion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Configuracion_Click(object sender, RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;

            MenuLateral menu = new MenuLateral("Perfil");

            Application.Current.MainWindow = menu;
            menu.Show();
            Close();
        }

        /// <summary>
        /// Handles the Click event of the CerrarSesion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void CerrarSesion_Click(object sender, RoutedEventArgs e)
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