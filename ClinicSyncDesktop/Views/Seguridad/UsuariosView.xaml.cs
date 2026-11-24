using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Views.Seguridad;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class UsuariosView : UserControl
    {
        /// <summary>
        /// The usuarios service
        /// </summary>
        private readonly UsuariosApiService? _usuariosService;
        /// <summary>
        /// The roles service
        /// </summary>
        private readonly RolesApiService _rolesService = new();
        /// <summary>
        /// The vista
        /// </summary>
        private ICollectionView? _vista;

        /// <summary>
        /// Fechas the relativa.
        /// </summary>
        /// <param name="fecha">The fecha.</param>
        /// <returns></returns>
        public static string FechaRelativa(DateTime fecha)
        {
            var timeSpan = DateTime.Now - fecha;

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                return timeSpan.Seconds > 1 ?
                    $"hace {timeSpan.Seconds} segundos" : "hace 1 segundo";
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                return timeSpan.Minutes > 1 ?
                    $"hace {timeSpan.Minutes} minutos" : "hace 1 minuto";
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                return timeSpan.Hours > 1 ?
                    $"hace {timeSpan.Hours} horas" : "hace 1 hora";
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                return timeSpan.Days > 1 ?
                    $"hace {timeSpan.Days} días" : "hace 1 día";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                return timeSpan.Days > 30 ?
                    $"hace {timeSpan.Days / 30} meses" : "hace 1 mes";
            }
            else
            {
                return timeSpan.Days > 365 ?
                    $"hace {timeSpan.Days / 365} años" : "hace 1 año";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsuariosView"/> class.
        /// </summary>
        public UsuariosView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsuariosView"/> class.
        /// </summary>
        /// <param name="usuariosService">The usuarios service.</param>
        public UsuariosView(UsuariosApiService usuariosService) : this()
        {
            _usuariosService = usuariosService;
            Loaded += UsuariosView_Loaded;
        }

        // Mostrar notificaciones con el mismo diseño que Pacientes.
        /// <summary>
        /// Mostrars the aviso.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="exitoso">if set to <c>true</c> [exitoso].</param>
        private void MostrarAviso(string mensaje, bool exitoso)
        {
            PanelAvisos.Visibility = Visibility.Visible;
            PanelAvisos.Children.Clear();
            var aviso = new AvisoNotificacion(mensaje, exitoso);
            PanelAvisos.Children.Add(aviso);
        }

        /// <summary>
        /// Handles the Loaded event of the UsuariosView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void UsuariosView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarUsuariosAsync();
        }

        /// <summary>
        /// Cargars the usuarios asynchronous.
        /// </summary>
        private async Task CargarUsuariosAsync()
        {
            if (_usuariosService is null)
            {
                MostrarAviso(
                    "La conexión con la API todavía no está configurada.", exitoso: false);
                return;
            }

            try
            {
                var listado = await _usuariosService.ListarAsync();
                Dictionary<int, string> nombresRoles = [];
                try
                {
                    var roles = await _rolesService.ListarAsync();
                    nombresRoles = roles.ToDictionary(r => r.IdRol, r => r.Nombre);
                }
                catch
                {
                    // Si no se pueden obtener roles adicionales, se usa el NombreRol provisto por el servidor
                }

                var usuarios = listado.Select(u =>
                {
                    var ultimoAcceso = u.UltimoAcceso ?? u.UltimoLogin;
                    return new UsuarioListado()
                    {
                        IdUsuario = u.IdUsuario,
                        IdRol = u.IdRol,
                        NombreRol = !string.IsNullOrWhiteSpace(u.NombreRol)
                            ? u.NombreRol
                            : (nombresRoles.TryGetValue(u.IdRol, out var nombre) ? nombre : $"Rol #{u.IdRol}"),
                        NombreCompleto = $"{u.Nombres} {u.Apellidos}",
                        UltimoAcceso = ultimoAcceso.HasValue
                            ? FechaRelativa(ultimoAcceso.Value)
                            : null,
                        Estado = u.Estado
                    };
                });

                var coleccion = new ObservableCollection<UsuarioListado>(usuarios);

                _vista = CollectionViewSource.GetDefaultView(coleccion);
                _vista.Filter = FiltrarUsuario;

                dgUsuarios.ItemsSource = _vista;
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc);
                MostrarAviso(
                    "No fue posible cargar los usuarios. Verifica el servidor y tus permisos.",
                    exitoso: false);
            }
        }

        /// <summary>
        /// Handles the Click event of the BtnAgregar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void BtnAgregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_usuariosService is null)
            {
                MostrarAviso(
                    "La conexión con la API todavía no está configurada.",
                    exitoso: false);
                return;
            }

            var ventana = new AgregarUsuarioWindow(
                _usuariosService,
                new RolesApiService())
            {
                Owner = Window.GetWindow(this)
            };

            var resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                await CargarUsuariosAsync();
                MostrarAviso("Usuario registrado exitosamente.", exitoso: true);
            }
        }

        /// <summary>
        /// Occurs when [volver al menu solicitado].
        /// </summary>
        public event EventHandler? VolverAlMenuSolicitado;

        /// <summary>
        /// Handles the Click event of the BtnVolver control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnVolver_Click(
            object sender,
            RoutedEventArgs e)
        {
            VolverAlMenuSolicitado?.Invoke(
                this,
                EventArgs.Empty);
        }

        /// <summary>
        /// Filtrars the usuario.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool FiltrarUsuario(object obj)
        {
            if (obj is not UsuarioListado usuario)
                return false;

            string texto = TxtBuscar.Text.Trim();

            if (string.IsNullOrEmpty(texto))
                return true;

            return usuario.NombreCompleto.Contains(texto, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handles the TextChanged event of the TxtBuscar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TxtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            _vista?.Refresh();
        }

        /// <summary>
        /// Handles the Click event of the BtnOpciones control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void BtnOpciones_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_usuariosService is null || sender is not Button boton || boton.DataContext is not UsuarioListado seleccionado)
                return;

            try
            {
                var detalle = await _usuariosService.ObtenerUsuarioAsync(seleccionado.IdUsuario);
                var ventana = new EditarUsuarioWindow(detalle, _usuariosService, new RolesApiService())
                {
                    Owner = Window.GetWindow(this)
                };

                if (ventana.ShowDialog() == true)
                {
                    await CargarUsuariosAsync();
                    MostrarAviso("Usuario actualizado correctamente.", exitoso: true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MostrarAviso("No fue posible abrir la edición del usuario.", exitoso: false);
            }
        }
    }
}