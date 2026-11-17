
using ClinicSyncDesktop.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClinicSyncDesktop.Views.Perfil._2FA;

namespace ClinicSyncDesktop.Views
{
    public partial class PerfilView : UserControl
    {
        private bool cargandoPerfil = false;
        private PerfilActual? perfilActual;
        private bool dosFAConfigurado = false;

        private void MostrarNotificacion(string mensaje, bool exitoso)
        {
            PanelAvisos.Children.Clear();

            PanelAvisos.Visibility = Visibility.Visible;

            PanelAvisos.Children.Add(
                new AvisoNotificacion(mensaje, exitoso));
        }

        // EVENTO PARA ABRIR EDICIÓN
        public event Action<PerfilActual>? EditarSolicitado;

        public PerfilView()
        {
            InitializeComponent();
            MostrarSeccion("datos");
        }

        // ==========================================
        // NAVEGACIÓN INTERNA
        // ==========================================

        private void MostrarSeccion(string seccion)
        {
            PanelDatosPersonales.Visibility =
                seccion == "datos"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            PanelSeguridad.Visibility =
                seccion == "seguridad"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            PanelAcercaDe.Visibility =
                seccion == "acerca"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            BtnDatosPersonales.FontWeight =
                seccion == "datos"
                    ? FontWeights.Bold
                    : FontWeights.Normal;

            BtnSeguridad.FontWeight =
                seccion == "seguridad"
                    ? FontWeights.Bold
                    : FontWeights.Normal;

            BtnAcercaDe.FontWeight =
                seccion == "acerca"
                    ? FontWeights.Bold
                    : FontWeights.Normal;

            Brush activo = new SolidColorBrush(
                Color.FromRgb(23, 71, 90));

            Brush inactivo = new SolidColorBrush(
                Color.FromRgb(34, 34, 34));

            BtnDatosPersonales.Foreground =
                seccion == "datos" ? activo : inactivo;

            BtnSeguridad.Foreground =
                seccion == "seguridad" ? activo : inactivo;

            BtnAcercaDe.Foreground =
                seccion == "acerca" ? activo : inactivo;
        }

        private void DatosPersonales_Click(
            object sender,
            RoutedEventArgs e)
        {
            MostrarSeccion("datos");
        }

        private void Seguridad_Click(
            object sender,
            RoutedEventArgs e)
        {
            MostrarSeccion("seguridad");
        }

        private void AcercaDe_Click(
            object sender,
            RoutedEventArgs e)
        {
            MostrarSeccion("acerca");
        }

        // ==========================================
        // CAMBIAR CONTRASEÑA
        // ==========================================

        private void CambiarPassword_Click(
            object sender,
            RoutedEventArgs e)
        {
            var ventana = new CambiarContrasenaWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (ventana.ShowDialog() == true)
            {
                MostrarNotificacion(
                    "Se cambió la contraseña.",
                    true);
            }
            else if (!string.IsNullOrWhiteSpace(ventana.ErrorCambio))
            {
                MostrarNotificacion(
                    "Error al cambiar la contraseña.",
                    false);
            }
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR AUTENTICACIÓN 2FA
        // ==========================================

        private async void Configurar2FA_Click(
            object sender,
            RoutedEventArgs e)
        {
            // ==========================================
            // SI EL 2FA YA ESTÁ CONFIGURADO: DESACTIVAR
            // ==========================================

            if (dosFAConfigurado)
            {
                ConfirmarDescarteWindow confirmacion =
                    new ConfirmarDescarteWindow(
                       "¿Deseas desactivar el 2FA? Ya no se solicitará " +
                        "la verificación facial al iniciar sesión.")
                    {
                        Owner = Window.GetWindow(this)
                    };

                bool? resultadoConfirmacion = confirmacion.ShowDialog();

                if (resultadoConfirmacion != true)
                    return;

                try
                {
                    await ClienteApi.Desactivar2FAAsync();

                    await CargarEstado2FAAsync();

                    PanelAvisos.Visibility = Visibility.Visible;

                    PanelAvisos.Children.Add(
                        new AvisoNotificacion(
                            "La autenticación de dos factores se desactivó correctamente.",
                            true));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "No se pudo desactivar la autenticación 2FA:\n\n" +
                        ex.Message,
                        "ClinicSync - Seguridad",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                return;
            }

            // ==========================================
            // SI EL 2FA NO ESTÁ CONFIGURADO: ACTIVAR
            // ==========================================

            Activar2FAWindow ventana = new Activar2FAWindow
            {
                Owner = Window.GetWindow(this)
            };

            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                await CargarEstado2FAAsync();

                if (dosFAConfigurado)
                {
                    PanelAvisos.Visibility = Visibility.Visible;

                    PanelAvisos.Children.Add(
                        new AvisoNotificacion(
                            "La autenticación de dos factores se activó correctamente.",
                            true));
                }
            }
        }

        // ==========================================
        // CARGAR ESTADO DE AUTENTICACIÓN 2FA
        // ==========================================

        private async Task CargarEstado2FAAsync()
        {
            try
            {
                Estado2FA estado =
                    await ClienteApi.ObtenerEstado2FAAsync();

                dosFAConfigurado = estado.Configurado;

                TxtEstado2FA.Text =
                    dosFAConfigurado
                        ? "Configurado"
                        : "No configurado";
                ActualizarVisual2FA();
            }
            catch (Exception ex)
            {
                TxtEstado2FA.Text = "No disponible";

                System.Diagnostics.Debug.WriteLine(
                    "Error al consultar el estado 2FA: " + ex.Message);
                dosFAConfigurado = false;
                ActualizarVisual2FA();
            }
        }

        // ==========================================
        // ACTUALIZAR INTERRUPTOR VISUAL 2FA
        // ==========================================

        private void ActualizarVisual2FA()
        {
            if (dosFAConfigurado)
            {
                // ESTADO ACTIVADO
                FondoInterruptor2FA.Background =
                    new SolidColorBrush(
                        Color.FromRgb(34, 152, 169));

                IndicadorInterruptor2FA.HorizontalAlignment =
                    HorizontalAlignment.Right;

                IndicadorInterruptor2FA.Margin =
                    new Thickness(0, 0, 3, 0);

                BtnInterruptor2FA.ToolTip =
                    "Autenticación de dos factores configurada";
            }
            else
            {
                // ESTADO DESACTIVADO
                FondoInterruptor2FA.Background =
                    new SolidColorBrush(
                        Color.FromRgb(217, 221, 218));

                IndicadorInterruptor2FA.HorizontalAlignment =
                    HorizontalAlignment.Left;

                IndicadorInterruptor2FA.Margin =
                    new Thickness(3, 0, 0, 0);

                BtnInterruptor2FA.ToolTip =
                    "Configuración de dos factores pendiente";
            }
        }

        // ==========================================
        // CARGAR PERFIL
        // ==========================================

        private async void PerfilView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (cargandoPerfil)
                return;

            cargandoPerfil = true;
            TxtError.Visibility = Visibility.Collapsed;

            try
            {
                PerfilActual perfil =
                    await ClienteApi.ObtenerPerfilAsync();

                perfilActual = perfil;

                TxtNombrePerfil.Text = perfil.NombreCorto;
                TxtRolPerfil.Text = FormatearRol(perfil.Rol);

                TxtIdentidad.Text = perfil.Identidad ?? "";
                TxtNombres.Text = perfil.Nombres ?? "";
                TxtApellidos.Text = perfil.Apellidos ?? "";

                if (perfil.FechaNacimiento != DateTime.MinValue)
                {
                    TxtFechaNacimiento.Text =
                        perfil.FechaNacimiento.ToString("yyyy/MM/dd");
                }
                else
                {
                    TxtFechaNacimiento.Text = "";
                }

                TxtSexo.Text = FormatearSexo(perfil.Genero);
                TxtTelefono.Text = perfil.Telefono ?? "";
                TxtCorreo.Text = perfil.CorreoContacto ?? "";

                await CargarFotografiaAsync();
                await CargarEstado2FAAsync();
            }
            catch (Exception ex)
            {
                TxtNombrePerfil.Text = "Perfil no disponible";
                TxtRolPerfil.Text = "";

                TxtError.Text =
                    "No se pudo cargar el perfil: " + ex.Message;

                TxtError.Visibility = Visibility.Visible;
            }
            finally
            {
                cargandoPerfil = false;
            }
        }

        // ==========================================
        // CARGAR FOTOGRAFÍA
        // ==========================================

        private async Task CargarFotografiaAsync()
        {
            try
            {
                byte[]? contenido =
                    await ClienteApi.DescargarMiImagenPerfilAsync();

                if (contenido == null || contenido.Length == 0)
                {
                    MostrarFotoPredeterminada();
                    return;
                }

                using MemoryStream stream =
                    new MemoryStream(contenido);

                BitmapImage imagen = new BitmapImage();

                imagen.BeginInit();
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.StreamSource = stream;
                imagen.EndInit();
                imagen.Freeze();

                ImgPerfil.Source = imagen;
            }
            catch
            {
                MostrarFotoPredeterminada();
            }
        }

        private void MostrarFotoPredeterminada()
        {
            ImgPerfil.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/Resources/ImagenesPerfil/perfil.png",
                    UriKind.Absolute));
        }

        // ==========================================
        // FORMATEAR ROL
        // ==========================================

        private string FormatearRol(string? rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                return "";

            if (rol.Equals(
                "ADMINISTRADOR",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Administrador";
            }

            if (rol.Equals(
                "EMPLEADO",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Empleado";
            }

            return rol;
        }

        // ==========================================
        // FORMATEAR SEXO
        // ==========================================

        private string FormatearSexo(string? genero)
        {
            if (string.IsNullOrWhiteSpace(genero))
                return "";

            switch (genero.ToUpperInvariant())
            {
                case "F":
                    return "Femenino";

                case "M":
                    return "Masculino";

                default:
                    return genero;
            }
        }

        // ==========================================
        // ABRIR EDICIÓN
        // ==========================================

        private void EditarFoto_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (perfilActual == null)
                return;

            EditarSolicitado?.Invoke(perfilActual);
        }
    }
}
