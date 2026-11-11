
using ClinicSyncDesktop.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views
{
    public partial class PerfilView : UserControl
    {
        private bool cargandoPerfil = false;
        private PerfilActual? perfilActual;

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
            MessageBox.Show(
                "El cambio de contraseña desde el perfil requiere " +
                "un endpoint autenticado en ClinicSyncServer. " +
                "Todavía no se ha conectado esta función.",
                "ClinicSync - Seguridad",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // ==========================================
        // CONFIGURAR 2FA
        // ==========================================
        private void Configurar2FA_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "La configuración de 2FA requiere consultar " +
                "el estado real de la cuenta y completar la " +
                "verificación antes de activarla. " +
                "Esta función todavía no está conectada.",
                "ClinicSync - Seguridad",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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
