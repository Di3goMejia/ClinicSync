
using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Views;
using ClinicSyncDesktop.Views.Notificaciones;
using ClinicSyncDesktop.Views.Seguridad;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ClinicSync.Views
{
    public partial class MenuLateral : Window
    {
        // BOTÓN ACTUALMENTE SELECCIONADO
        private Button? botonSeleccionado;

        // SERVICIOS
        private readonly TratamientosApiService tratamientosService = new();
        private readonly NotificacionesApiService notificacionesService = new();

        // NOTIFICACIONES
        private readonly DispatcherTimer relojNotificaciones =
            new() { Interval = TimeSpan.FromMinutes(1) };

        private bool cargandoNotificaciones;

        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        public MenuLateral(string modulo = "Pacientes")
        {
            InitializeComponent();

            PrepararNotificaciones();

            AbrirModulo(modulo);
        }

        // ==========================================
        // OBTENER USUARIO Y ROL
        // ==========================================

        private async void Ventana_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await ActualizarDatosUsuarioAsync();
        }

        // ==========================================
        // ACTUALIZAR DATOS DEL USUARIO
        // ==========================================

        private async Task ActualizarDatosUsuarioAsync()
        {
            try
            {
                PerfilActual perfil =
                    await ClienteApi.ObtenerPerfilAsync();

                NombrePerfil.Text = perfil.NombreCorto;

                RolPerfil.Text = perfil.Rol;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "Error al cargar el perfil: " +
                    ex.Message);

                NombrePerfil.Text = "Perfil no disponible";

                RolPerfil.Text = "";
            }
        }

        // ==========================================
        // BOTONES DEL MENÚ
        // ==========================================

        private void Modulo_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton)
            {
                string modulo =
                    boton.Tag?.ToString() ?? "Inicio";

                AbrirModulo(modulo);
            }
        }

        // ==========================================
        // ABRIR MÓDULOS
        // ==========================================

        public void AbrirModulo(string modulo)
        {
            switch (modulo)
            {
                case "Inicio":

                    MenuPrincipal inicio = new();

                    Application.Current.MainWindow = inicio;

                    inicio.Show();

                    Close();

                    break;

                case "Pacientes":

                    ContenidoPrincipal.Content =
                        new PacientesView();

                    break;

                case "Tratamientos":

                    ContenidoPrincipal.Content =
                        new TratamientosView(
                            tratamientosService);

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
                        new SeguridadView();

                    break;

                case "Notificaciones":

                    ContenidoPrincipal.Content =
                        new NotificacionesView();

                    break;

                case "Perfil":

                    MostrarPerfil();

                    break;

                default:

                    ContenidoPrincipal.Content =
                        CrearVistaTemporal(modulo);

                    break;
            }

            // IDENTIFICAR BOTÓN DEL MENÚ
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

        // ==========================================
        // RESALTAR OPCIÓN ACTIVA
        // ==========================================

        private void ResaltarBoton(Button? boton)
        {
            if (botonSeleccionado != null)
            {
                botonSeleccionado.Background =
                    Brushes.Transparent;

                botonSeleccionado.Foreground =
                    Brushes.White;
            }

            botonSeleccionado = null;

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

        // ==========================================
        // VISTA TEMPORAL
        // ==========================================

        private static UserControl CrearVistaTemporal(
            string modulo)
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

        // ==========================================
        // ABRIR EXPEDIENTE
        // ==========================================

        public void AbrirExpediente(int idPaciente)
        {
            ContenidoPrincipal.Content =
                new ExpedientesView(idPaciente);

            ResaltarBoton(btnPacientes);
        }

        // ==========================================
        // VOLVER A PACIENTES
        // ==========================================

        public void VolverAPacientes()
        {
            AbrirModulo("Pacientes");
        }

        // ==========================================
        // CAMPANA DE NOTIFICACIONES
        // ==========================================

        private async void Campana_Click(
            object sender,
            RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;

            PopupNotificaciones.IsOpen =
                !PopupNotificaciones.IsOpen;

            if (PopupNotificaciones.IsOpen)
            {
                await CargarNotificacionesAsync();
            }
        }

        // ==========================================
        // BOTÓN DE USUARIO
        // ==========================================

        private void Usuario_Click(
            object sender,
            RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;

            PopupPerfil.IsOpen =
                !PopupPerfil.IsOpen;
        }

        // ==========================================
        // MARCAR NOTIFICACIONES COMO LEÍDAS
        // ==========================================

        private async void MarcarLeidas_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await notificacionesService
                    .MarcarTodasAsync();

                await CargarNotificacionesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        // ==========================================
        // VER TODAS LAS NOTIFICACIONES
        // ==========================================

        private void VerTodas_Click(
            object sender,
            RoutedEventArgs e)
        {
            PopupNotificaciones.IsOpen = false;

            AbrirModulo("Notificaciones");
        }

        // ==========================================
        // ICONO DE CONFIGURACIÓN
        // ==========================================

        private void Configuracion_Click(
            object sender,
            RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;

            AbrirModulo("Perfil");
        }

        // ==========================================
        // CERRAR SESIÓN
        // ==========================================

        private async void CerrarSesion_Click(
            object sender,
            RoutedEventArgs e)
        {
            PopupPerfil.IsOpen = false;

            try
            {
                await ClienteApi.CerrarSesionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cerrar sesión: " +
                    ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Login login = new();

            Application.Current.MainWindow = login;

            login.Show();

            Close();
        }

        // ==========================================
        // PREPARAR NOTIFICACIONES
        // ==========================================

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

        // ==========================================
        // CARGAR NOTIFICACIONES
        // ==========================================

        private async Task CargarNotificacionesAsync()
        {
            if (cargandoNotificaciones)
                return;

            cargandoNotificaciones = true;

            try
            {
                var todas =
                    await notificacionesService.ListarAsync();

                // NOTIFICACIONES VISIBLES
                var visibles = todas
                    .Where(n => n.Vigente && !n.Archivada && !n.Leida)
                    .OrderByDescending(n => n.FechaCreacion)
                    .Take(8)
                    .ToList();

                // INDICADOR DE PENDIENTES
                PuntoPendientes.Visibility =
                    todas.Any(n =>
                        !n.Leida &&
                        n.Vigente &&
                        !n.Archivada)
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                // LIMPIAR LISTAS
                ListaAhora.Children.Clear();

                ListaAnteriores.Children.Clear();

                // DISTRIBUIR NOTIFICACIONES
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
                                await notificacionesService
                                    .MarcarLeidaAsync(
                                        n.IdNotificacion);

                                await CargarNotificacionesAsync();
                            },

                            null));
                }

                // MOSTRAR SECCIÓN AHORA
                TituloAhora.Visibility =
                    ListaAhora.Children.Count > 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                // MOSTRAR SECCIÓN ANTERIORES
                TituloAnteriores.Visibility =
                    ListaAnteriores.Children.Count > 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;

                // MENSAJE SIN NOTIFICACIONES
                MensajeSinNotificaciones.Visibility =
                    visibles.Count == 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "Error al cargar notificaciones: " +
                    ex.Message);
            }
            finally
            {
                cargandoNotificaciones = false;
            }
        }

        // ==========================================
        // MOSTRAR PERFIL EN MODO CONSULTA
        // ==========================================

        private void MostrarPerfil()
        {
            // CREAR VISTA DE SOLO LECTURA
            PerfilView vista = new PerfilView();

            // CONECTAR LÁPIZ CON EDICIÓN
            vista.EditarSolicitado += AbrirEditarPerfil;

            // MOSTRAR PERFIL
            ContenidoPrincipal.Content = vista;
        }

        // ==========================================
        // ABRIR PERFIL EN MODO EDICIÓN
        // ==========================================

        private void AbrirEditarPerfil(
            PerfilActual perfil)
        {
            // CREAR USERCONTROL DE EDICIÓN
            EditarPerfilView vista =
                new EditarPerfilView(perfil);

            // CANCELAR: VOLVER SIN GUARDAR
            vista.Cancelado += () =>
            {
                MostrarPerfil();
            };

            // GUARDAR: VOLVER Y RECARGAR
            vista.Guardado += async () =>
            {
                // REGRESAR A VISTA DE CONSULTA
                MostrarPerfil();

                // ACTUALIZAR DATOS DEL MENÚ
                await ActualizarDatosUsuarioAsync();
            };

            // MOSTRAR EDICIÓN
            ContenidoPrincipal.Content = vista;
        }
    }
}
