
using ClinicSyncDesktop.Data;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSync.Views
{
    public partial class MenuPrincipal : Window
    {
        public MenuPrincipal()
        {
            InitializeComponent();
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

        private void Campana_Click(
            object sender, RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;
            PopupNotificaciones.IsOpen =
                !PopupNotificaciones.IsOpen;
        }

        private void PerfilMenu_Click(
            object sender, RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;
            PopupPerfil.IsOpen = !PopupPerfil.IsOpen;
        }

        private void MarcarLeidas_Click(
            object sender, RoutedEventArgs e)
        {
            // Pendiente de conectar con notificaciones.
        }

        private void VerTodas_Click(
            object sender, RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;

            MenuLateral menu =
                new MenuLateral("Notificaciones");

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
