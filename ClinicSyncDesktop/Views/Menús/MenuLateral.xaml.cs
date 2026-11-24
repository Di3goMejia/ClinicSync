
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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MenuLateral : Window
    {
        // BOTÓN ACTUALMENTE SELECCIONADO
        /// <summary>
        /// The boton seleccionado
        /// </summary>
        private Button? botonSeleccionado;

        // SERVICIOS
        /// <summary>
        /// The tratamientos service
        /// </summary>
        private readonly TratamientosApiService tratamientosService = new();
        /// <summary>
        /// The notificaciones service
        /// </summary>
        private readonly NotificacionesApiService notificacionesService = new();

        // NOTIFICACIONES
        /// <summary>
        /// The reloj notificaciones
        /// </summary>
        private readonly DispatcherTimer relojNotificaciones =
            new() { Interval = TimeSpan.FromMinutes(1) };

        /// <summary>
        /// The cargando notificaciones
        /// </summary>
        private bool cargandoNotificaciones;

        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuLateral"/> class.
        /// </summary>
        /// <param name="modulo">The modulo.</param>
        public MenuLateral(string modulo = "Pacientes")
        {
            InitializeComponent();

            PrepararNotificaciones();

            AbrirModulo(modulo);
        }

        // ==========================================
        // OBTENER USUARIO Y ROL
        // ==========================================

        /// <summary>
        /// Handles the Loaded event of the Ventana control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Ventana_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await ActualizarDatosUsuarioAsync();
        }

        // ==========================================
        // ACTUALIZAR DATOS DEL USUARIO
        // ==========================================

        /// <summary>
        /// Actualizars the datos usuario asynchronous.
        /// </summary>
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

        /// <summary>
        /// Handles the Click event of the Modulo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Abrirs the modulo.
        /// </summary>
        /// <param name="modulo">The modulo.</param>
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

        /// <summary>
        /// Resaltars the boton.
        /// </summary>
        /// <param name="boton">The boton.</param>
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

        /// <summary>
        /// Crears the vista temporal.
        /// </summary>
        /// <param name="modulo">The modulo.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Abrirs the expediente.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        public void AbrirExpediente(int idPaciente)
        {
            ContenidoPrincipal.Content =
                new ExpedientesView(idPaciente);

            ResaltarBoton(btnPacientes);
        }

        // ==========================================
        // VOLVER A PACIENTES
        // ==========================================

        /// <summary>
        /// Volvers a pacientes.
        /// </summary>
        public void VolverAPacientes()
        {
            AbrirModulo("Pacientes");
        }

        // ==========================================
        // CAMPANA DE NOTIFICACIONES
        // ==========================================

        /// <summary>
        /// Handles the Click event of the Campana control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the Usuario control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the MarcarLeidas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the VerTodas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the Configuracion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the CerrarSesion control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

            Closed += (_, _) =>
                relojNotificaciones.Stop();
        }

        // ==========================================
        // CARGAR NOTIFICACIONES
        // ==========================================

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

        /// <summary>
        /// Mostrars the perfil.
        /// </summary>
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

        /// <summary>
        /// Abrirs the editar perfil.
        /// </summary>
        /// <param name="perfil">The perfil.</param>
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
