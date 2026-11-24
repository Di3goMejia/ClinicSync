
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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class PerfilView : UserControl
    {
        /// <summary>
        /// The cargando perfil
        /// </summary>
        private bool cargandoPerfil = false;
        /// <summary>
        /// The perfil actual
        /// </summary>
        private PerfilActual? perfilActual;
        /// <summary>
        /// The dos fa configurado
        /// </summary>
        private bool dosFAConfigurado = false;

        /// <summary>
        /// Mostrars the notificacion.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="exitoso">if set to <c>true</c> [exitoso].</param>
        private void MostrarNotificacion(string mensaje, bool exitoso)
        {
            PanelAvisos.Children.Clear();

            PanelAvisos.Visibility = Visibility.Visible;

            PanelAvisos.Children.Add(
                new AvisoNotificacion(mensaje, exitoso));
        }

        // EVENTO PARA ABRIR EDICIÓN
        /// <summary>
        /// Occurs when [editar solicitado].
        /// </summary>
        public event Action<PerfilActual>? EditarSolicitado;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerfilView"/> class.
        /// </summary>
        public PerfilView()
        {
            InitializeComponent();
            MostrarSeccion("datos");
        }

        // ==========================================
        // NAVEGACIÓN INTERNA
        // ==========================================

        /// <summary>
        /// Mostrars the seccion.
        /// </summary>
        /// <param name="seccion">The seccion.</param>
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

        /// <summary>
        /// Handles the Click event of the DatosPersonales control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void DatosPersonales_Click(
            object sender,
            RoutedEventArgs e)
        {
            MostrarSeccion("datos");
        }

        /// <summary>
        /// Handles the Click event of the Seguridad control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Seguridad_Click(
            object sender,
            RoutedEventArgs e)
        {
            MostrarSeccion("seguridad");
        }

        /// <summary>
        /// Handles the Click event of the AcercaDe control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AcercaDe_Click(
            object sender,
            RoutedEventArgs e)
        {
            MostrarSeccion("acerca");
        }

        // ==========================================
        // CAMBIAR CONTRASEÑA
        // ==========================================

        /// <summary>
        /// Handles the Click event of the CambiarPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the Configurar2FA control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Cargars the estado2 fa asynchronous.
        /// </summary>
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

        /// <summary>
        /// Actualizars the visual2 fa.
        /// </summary>
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

        /// <summary>
        /// Handles the Loaded event of the PerfilView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Cargars the fotografia asynchronous.
        /// </summary>
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

        /// <summary>
        /// Mostrars the foto predeterminada.
        /// </summary>
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

        /// <summary>
        /// Formatears the rol.
        /// </summary>
        /// <param name="rol">The rol.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Formatears the sexo.
        /// </summary>
        /// <param name="genero">The genero.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handles the Click event of the EditarFoto control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void EditarFoto_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (perfilActual == null)
                return;

            EditarSolicitado?.Invoke(perfilActual);
        }

        /// <summary>
        /// Handles the TextChanged event of the TxtNombres control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TxtNombres_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
