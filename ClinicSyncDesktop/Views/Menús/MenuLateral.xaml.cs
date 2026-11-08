using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Views.Notificaciones;
using System.Threading.Tasks;
using System.Windows.Threading;
using ClinicSyncDesktop.Views.Notificaciones;



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
            PrepararNotificaciones();
            AbrirModulo("Pacientes");
        }

        public MenuLateral(string modulo)
        {
            InitializeComponent();
            PrepararNotificaciones();
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
                        new InventarioView();
                    break;

                case "Finanzas":
                    ContenidoPrincipal.Content =
                        new FinanzasView();
                    break;

                case "Agenda":
                    ContenidoPrincipal.Content =
                        new AgendaView();
                    break;

                case "Seguridad":
                    ContenidoPrincipal.Content =
                         CrearVistaTemporal("Seguridad");
                    break;

                case "Notificaciones":
                    ContenidoPrincipal.Content = new NotificacionesView();
                    break;

                    return;


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

        // PERFIL
        private void Usuario_Click(
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
            object sender,
            RoutedEventArgs e)
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


    }
}
