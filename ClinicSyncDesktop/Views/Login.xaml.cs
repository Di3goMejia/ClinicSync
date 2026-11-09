using ClinicSyncDesktop.Data;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSync.Views
{
    public partial class Login : Window
    {
        private bool contrasenaVisible = false;
        private bool iniciandoSesion = false;

        public Login()
        {
            InitializeComponent();

            TxtUsuario.Clear();
            TxtContrasena.Clear();
            TxtContrasenaVisible.Clear();

            TxtContrasena.Visibility = Visibility.Visible;
            TxtContrasenaVisible.Visibility = Visibility.Collapsed;
        }

        // MOSTRAR U OCULTAR CONTRASEÑA
        private void BtnMostrarContrasena_Click(
            object sender, RoutedEventArgs e)
        {
            contrasenaVisible = !contrasenaVisible;

            if (contrasenaVisible)
            {
                TxtContrasenaVisible.Text = TxtContrasena.Password;

                TxtContrasena.Visibility = Visibility.Collapsed;
                TxtContrasenaVisible.Visibility = Visibility.Visible;

                BtnMostrarContrasena.ToolTip = "Ocultar contraseña";

                TxtContrasenaVisible.Focus();
                TxtContrasenaVisible.CaretIndex =
                    TxtContrasenaVisible.Text.Length;
            }
            else
            {
                TxtContrasena.Password = TxtContrasenaVisible.Text;

                TxtContrasenaVisible.Visibility = Visibility.Collapsed;
                TxtContrasena.Visibility = Visibility.Visible;

                BtnMostrarContrasena.ToolTip = "Mostrar contraseña";

                TxtContrasena.Focus();
            }
        }

        // INICIAR SESIÓN MEDIANTE LA API
        private async void Acceder_Click(
            object sender, RoutedEventArgs e)
        {
            if (iniciandoSesion)
                return;

            string usuario = TxtUsuario.Text.Trim();

            string password = contrasenaVisible
                ? TxtContrasenaVisible.Text
                : TxtContrasena.Password;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MessageBox.Show("Ingrese su usuario.", "ClinicSync");
                TxtUsuario.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Ingrese su contraseña.", "ClinicSync");
                return;
            }

            iniciandoSesion = true;

            Button? boton = sender as Button;
            boton?.IsEnabled = false;

            try
            {
                var resultado = await ClienteApi.IniciarSesionAsync(
                    usuario, 
                    password);

                if (resultado.Requiere2fa)
                {
                    MessageBox.Show(
                        "Esta cuenta requiere verificación en dos pasos. " +
                        "El servidor todavía tiene pendiente implementar " +
                        "la comprobación del segundo factor.",
                        "ClinicSync",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                // Comprobar que la sesión permite consultar el perfil.
                await ClienteApi.ObtenerPerfilAsync();

                MenuPrincipal menu = new();

                Application.Current.MainWindow = menu;
                menu.Show();

                Close();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "No se pudo conectar con ClinicSyncServer.\n\n" +
                    "Verificá que el servidor esté encendido en " +
                    "http://localhost:5227/.\n\n" +
                    ex.Message + "\n\n" +
                    ex.InnerException?.Message,
                    "Error de conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "No se pudo iniciar sesión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            finally
            {
                TxtContrasena.Clear();
                TxtContrasenaVisible.Clear();

                iniciandoSesion = false;
                boton?.IsEnabled = true;
            }
        }

        private void OlvidoContrasena_Click(
        object sender, RoutedEventArgs e)
        {
            var ventana =
                new ClinicSyncDesktop.Views.RecuperacionContrasenaView();

            ventana.Owner = this;

            ventana.ShowDialog();
        }

        private void ActivarCuenta_Click(
        object sender, RoutedEventArgs e)
        {
            var ventana =
                new ClinicSyncDesktop.Views.ActivacionCuentaView();

            ventana.Owner = this;

            ventana.ShowDialog();
        }

        private void ChkRecordarSesion_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}