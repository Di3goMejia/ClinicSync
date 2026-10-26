
using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSync.Views
{
    public partial class MenuLateral : Window
    {
        private Button? botonSeleccionado;

        // SERVICIO PARA TRATAMIENTOS
        private readonly TratamientosApiService tratamientosService =
            new TratamientosApiService();

        public MenuLateral()
        {
            InitializeComponent();
            AbrirModulo("Pacientes");
        }

        public MenuLateral(string modulo)
        {
            InitializeComponent();
            AbrirModulo(modulo);
        }

        // OBTENER USUARIO Y ROL
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

        // BOTONES DEL MENÚ
        private void Modulo_Click(
            object sender, RoutedEventArgs e)
        {
            if (sender is Button boton)
            {
                string modulo =
                    boton.Tag?.ToString() ?? "Inicio";

                AbrirModulo(modulo);
            }
        }

        // ABRIR MÓDULOS
        public void AbrirModulo(string modulo)
        {
            if (modulo == "Inicio")
            {
                MenuPrincipal inicio = new MenuPrincipal();

                Application.Current.MainWindow = inicio;
                inicio.Show();
                Close();
                return;
            }

            switch (modulo)
            {
                case "Pacientes":
                    ContenidoPrincipal.Content =
                        new PacientesView();
                    break;

                case "Tratamientos":
                    ContenidoPrincipal.Content =
                        new TratamientosView(tratamientosService);
                    break;

                case "Inventario":
                    ContenidoPrincipal.Content =
                        CrearVistaTemporal("Inventario");
                    break;

                case "Finanzas":
                    ContenidoPrincipal.Content =
                        CrearVistaTemporal("Finanzas");
                    break;

                case "Agenda":
                    ContenidoPrincipal.Content =
                        CrearVistaTemporal("Agenda");
                    break;

                case "Seguridad":
                    ContenidoPrincipal.Content =
                        CrearVistaTemporal("Seguridad");
                    break;

                case "Notificaciones":
                    ContenidoPrincipal.Content =
                        CrearVistaTemporal("Notificaciones");
                    break;

                default:
                    ContenidoPrincipal.Content =
                        CrearVistaTemporal(modulo);
                    break;
            }

            Button? boton = modulo switch
            {
                "Pacientes" => btnPacientes,
                "Tratamientos" => btnTratamientos,
                "Inventario" => btnInventario,
                "Finanzas" => btnFinanzas,
                "Agenda" => btnAgenda,
                "Seguridad" => btnSeguridad,
                "Notificaciones" => btnNotificaciones,
                _ => null
            };

            ResaltarBoton(boton);
        }

        // RESALTAR OPCIÓN ACTIVA
        private void ResaltarBoton(Button? boton)
        {
            if (botonSeleccionado != null)
            {
                botonSeleccionado.Background =
                    Brushes.Transparent;

                botonSeleccionado.Foreground =
                    Brushes.White;
            }

            if (boton != null)
            {
                boton.Background =
                    new SolidColorBrush(
                        Color.FromRgb(146, 212, 222));

                boton.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(23, 71, 90));

                botonSeleccionado = boton;
            }
        }

        // VISTA TEMPORAL
        private UserControl CrearVistaTemporal(string modulo)
        {
            return new UserControl
            {
                Content = new Grid
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = modulo,
                            FontSize = 28,
                            FontWeight = FontWeights.Bold,
                            Foreground =
                                new SolidColorBrush(
                                    Color.FromRgb(23, 71, 90)),
                            HorizontalAlignment =
                                HorizontalAlignment.Center,
                            VerticalAlignment =
                                VerticalAlignment.Center
                        }
                    }
                }
            };
        }

        // ABRIR EXPEDIENTE
        public void AbrirExpediente(int idPaciente)
        {
            ContenidoPrincipal.Content =
                new ExpedientesView(idPaciente);

            ResaltarBoton(btnPacientes);
        }

        // VOLVER A PACIENTES
        public void VolverAPacientes()
        {
            AbrirModulo("Pacientes");
        }

        // NOTIFICACIONES
        private void Campana_Click(
            object sender, RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;
            PopupNotificaciones.IsOpen =
                !PopupNotificaciones.IsOpen;
        }

        // PERFIL
        private void Usuario_Click(
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
            AbrirModulo("Notificaciones");
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

        // CERRAR SESIÓN
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
