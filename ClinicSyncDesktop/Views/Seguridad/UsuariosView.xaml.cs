using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
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
    public partial class UsuariosView : UserControl
    {
        private readonly UsuariosApiService? _usuariosService;
        private ICollectionView? _vista;

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

        public UsuariosView()
        {
            InitializeComponent();
        }

        public UsuariosView(UsuariosApiService usuariosService) : this()
        {
            _usuariosService = usuariosService;
            Loaded += UsuariosView_Loaded;
        }

        // Mostrar notificaciones con el mismo diseño que Pacientes.
        private void MostrarAviso(string mensaje, bool exitoso)
        {
            PanelAvisos.Children.Clear();
            var aviso = new AvisoNotificacion(mensaje, exitoso);
            PanelAvisos.Children.Add(aviso);
        }

        private async void UsuariosView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await CargarUsuariosAsync();
        }

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

                var usuarios = listado.Select(u => new UsuarioListado()
                {
                    IdUsuario = u.IdUsuario,
                    IdRol = u.IdRol,
                    NombreCompleto = $"{u.Nombres} {u.Apellidos}",
                    UltimoAcceso = u.UltimoAcceso != DateTime.MinValue 
                        ? FechaRelativa(u.UltimoAcceso)
                        : null,
                    Estado = u.Estado
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

        private async void BtnAgregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            //if (_usuariosService is null)
            //{
            //    MostrarAviso(
            //        "La conexión con la API todavía no está configurada.",
            //        false);

            //    return;
            //}

            //var ventana = new AgregarTratamientoWindow(
            //    _usuariosService)
            //{
            //    Owner = Window.GetWindow(this)
            //};

            //bool? resultado = ventana.ShowDialog();

            //if (resultado == true)
            //{
            //    await CargarUsuariosAsync();

            //    MostrarAviso(
            //        "Tratamiento registrado exitosamente.",
            //        true);
            //}
            //else if (ventana.ErrorAlRegistrar)
            //{
            //    MostrarAviso(
            //        ventana.MensajeError,
            //        false);
            //}
        }

        public event EventHandler? VolverAlMenuSolicitado;

        private void BtnVolver_Click(
            object sender,
            RoutedEventArgs e)
        {
            VolverAlMenuSolicitado?.Invoke(
                this,
                EventArgs.Empty);
        }

        private bool FiltrarUsuario(object obj)
        {
            if (obj is not UsuarioListado usuario)
                return false;

            string texto = TxtBuscar.Text.Trim();

            if (string.IsNullOrEmpty(texto))
                return true;

            return usuario.NombreCompleto.Contains(texto, StringComparison.OrdinalIgnoreCase);
        }

        private void TxtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            _vista?.Refresh();
        }

        private async void BtnOpciones_Click(
            object sender,
            RoutedEventArgs e)
        {
            /*
            if (sender is not Button boton ||
                boton.DataContext is not Tratamiento tratamiento)
            {
                return;
            }

            if (_usuariosService is null)
            {
                MostrarAviso(
                    "La conexión con la API todavía no está configurada.",
                    false);

                return;
            }

            var ventana = new EditarTratamientoWindow(
                tratamiento,
                _usuariosService)
            {
                Owner = Window.GetWindow(this)
            };

            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                await CargarUsuariosAsync();

                MostrarAviso(
                    "Tratamiento actualizado exitosamente.",
                    true);
            }
            else if (ventana.ErrorAlGuardar)
            {
                MostrarAviso(
                    ventana.MensajeError,
                    false);
            }
            */
        }
    }
}