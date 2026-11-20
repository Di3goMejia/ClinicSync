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
    public partial class MenuPrincipal : Window
    {
        private readonly NotificacionesApiService servicioNotificaciones = new();

        private readonly DispatcherTimer relojNotificaciones = new()
        {
            Interval = TimeSpan.FromMinutes(1)
        };

        private bool cargandoNotificaciones;
        private bool esAdministrador;

        public MenuPrincipal()
        {
            InitializeComponent();
            PrepararNotificaciones();
        }

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

        private async Task CargarNotificacionesAsync()
        {
            if (cargandoNotificaciones)
                return;

            cargandoNotificaciones = true;

            try
            {
                var todas = await servicioNotificaciones.ListarAsync();

                var visibles = todas
                    .Where(n => n.Vigente && !n.Archivada)
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

        private async void Campana_Click(object sender, RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;
            PopupNotificaciones.IsOpen = !PopupNotificaciones.IsOpen;

            if (PopupNotificaciones.IsOpen)
            {
                await CargarNotificacionesAsync();
            }
        }

        private void PerfilMenu_Click(object sender, RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;
            PopupPerfil.IsOpen = !PopupPerfil.IsOpen;
        }

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

        private void VerTodas_Click(object sender, RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;

            MenuLateral menu = new MenuLateral("Notificaciones");

            Application.Current.MainWindow = menu;
            menu.Show();
            Close();
        }

        private void Configuracion_Click(object sender, RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;

            MenuLateral menu = new MenuLateral("Perfil");

            Application.Current.MainWindow = menu;
            menu.Show();
            Close();
        }

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