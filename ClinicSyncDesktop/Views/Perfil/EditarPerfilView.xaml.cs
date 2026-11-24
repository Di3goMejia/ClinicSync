
using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Validations;
using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class EditarPerfilView : UserControl
    {
        // DATOS DEL USUARIO AUTENTICADO
        /// <summary>
        /// The perfil
        /// </summary>
        private readonly PerfilActual perfil;

        // IMAGEN SELECCIONADA
        /// <summary>
        /// The ruta imagen seleccionada
        /// </summary>
        private string? rutaImagenSeleccionada;

        // CONTROL DE GUARDADO
        /// <summary>
        /// The guardando
        /// </summary>
        private bool guardando = false;

        // CONTROL DE CARGA DE FOTOGRAFÍAS
        /// <summary>
        /// The version imagen
        /// </summary>
        private int versionImagen = 0;

        // FOTOGRAFÍA QUE YA EXISTE EN EL SERVIDOR
        /// <summary>
        /// The fotografia guardada
        /// </summary>
        private byte[]? fotografiaGuardada;

        // Solo oculta la imagen durante esta edición, sin borrarla del servidor.
        /// <summary>
        /// The imagen oculta temporalmente
        /// </summary>
        private bool imagenOcultaTemporalmente = false;

        // EVENTOS DE NAVEGACIÓN
        /// <summary>
        /// Occurs when [cancelado].
        /// </summary>
        public event Action? Cancelado;
        /// <summary>
        /// Occurs when [guardado].
        /// </summary>
        public event Action? Guardado;

        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        /// <summary>
        /// Initializes a new instance of the <see cref="EditarPerfilView"/> class.
        /// </summary>
        /// <param name="perfilActual">The perfil actual.</param>
        public EditarPerfilView(PerfilActual perfilActual)
        {
            InitializeComponent();

            perfil = perfilActual;

            CargarDatos();

            MostrarImagenPredeterminada();

            Loaded += EditarPerfilView_Loaded;
        }

        // ==========================================
        // CARGAR DATOS DEL PERFIL
        // ==========================================

        /// <summary>
        /// Cargars the datos.
        /// </summary>
        private void CargarDatos()
        {
            TxtNombrePerfil.Text = perfil.NombreCorto;

            TxtRolPerfil.Text =
                perfil.Rol.Equals(
                    "ADMINISTRADOR",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Administrador"
                    : perfil.Rol.Equals(
                        "EMPLEADO",
                        StringComparison.OrdinalIgnoreCase)
                        ? "Empleado"
                        : perfil.Rol;

            TxtIdentidad.Text =
                perfil.Identidad ?? "";

            TxtNombres.Text =
                perfil.Nombres ?? "";

            TxtApellidos.Text =
                perfil.Apellidos ?? "";

            TxtTelefono.Text =
                perfil.Telefono ?? "";

            TxtCorreo.Text =
                perfil.CorreoContacto ?? "";

            // FECHA DE NACIMIENTO
            if (perfil.FechaNacimiento != DateTime.MinValue)
            {
                DpFechaNacimiento.SelectedDate =
                    perfil.FechaNacimiento;
            }

            // SEXO
            if (perfil.Genero == "F")
            {
                CmbSexo.SelectedIndex = 0;
            }
            else if (perfil.Genero == "M")
            {
                CmbSexo.SelectedIndex = 1;
            }

            // NO HAY IMAGEN NUEVA SELECCIONADA
            rutaImagenSeleccionada = null;

            BtnEliminarImagen.IsEnabled = false;
        }

        // ==========================================
        // CARGAR FOTOGRAFÍA GUARDADA
        // ==========================================

        private async void EditarPerfilView_Loaded(object sender, RoutedEventArgs e)
        {
            int versionActual = versionImagen;
            try
            {
                byte[]? contenido = await ClienteApi.DescargarMiImagenPerfilAsync();
                if (versionActual != versionImagen || rutaImagenSeleccionada != null || imagenOcultaTemporalmente)
                    return;

                fotografiaGuardada = contenido is { Length: > 0 } ? contenido : null;
                MostrarFotografiaGuardada();
            }
            catch (Exception ex)
            {
                if (versionActual != versionImagen || rutaImagenSeleccionada != null || imagenOcultaTemporalmente)
                    return;

                // Un error de conexión no significa que la fotografía se eliminó.
                MostrarError("No se pudo cargar la fotografía guardada: " + ex.Message);
            }
        }

        private void MostrarFotografiaGuardada()
        {
            if (imagenOcultaTemporalmente)
            {
                MostrarImagenPredeterminada();
                BtnEliminarImagen.IsEnabled = false;
                return;
            }

            if (fotografiaGuardada is { Length: > 0 })
            {
                BitmapImage imagen = ConvertirImagen(fotografiaGuardada);
                ImgPerfil.Source = imagen;
                ImgVistaPrevia.Source = imagen;
            }
            else
            {
                MostrarImagenPredeterminada();
            }

            BtnEliminarImagen.IsEnabled =
                !guardando && (rutaImagenSeleccionada != null || fotografiaGuardada != null);
        }

        // ==========================================
        // CONVERTIR BYTES EN IMAGEN
        // ==========================================

        private BitmapImage ConvertirImagen(
            byte[] contenido)
        {
            using MemoryStream stream =
                new MemoryStream(contenido);

            BitmapImage imagen =
                new BitmapImage();

            imagen.BeginInit();

            imagen.CacheOption =
                BitmapCacheOption.OnLoad;

            imagen.StreamSource = stream;

            imagen.EndInit();

            imagen.Freeze();

            return imagen;
        }

        // ==========================================
        // MOSTRAR IMAGEN SELECCIONADA
        // ==========================================

        private void MostrarImagen(
            byte[] contenido)
        {
            BitmapImage imagen =
                ConvertirImagen(contenido);

            // FOTOGRAFÍA SUPERIOR
            ImgPerfil.Source = imagen;

            // VISTA PREVIA DEL FORMULARIO
            ImgVistaPrevia.Source = imagen;
        }

        // ==========================================
        // IMAGEN PREDETERMINADA DEL PERFIL
        // ==========================================

        private void MostrarFotoPerfilPredeterminada()
        {
            ImgPerfil.Source = new BitmapImage(
                new Uri(
                    "pack://application:,,,/Resources/ImagenesPerfil/perfil.png",
                    UriKind.Absolute));
        }

        // ==========================================
        // IMAGEN PREDETERMINADA DEL FORMULARIO
        // ==========================================

        private void MostrarVistaPreviaPredeterminada()
        {
            ImgVistaPrevia.Source =
                new BitmapImage(
                    new Uri(
                        "/Resources/ImagenesPerfil/subir-imagen-perfil.png",
                        UriKind.Relative));
        }

        // ==========================================
        // RESTAURAR AMBAS IMÁGENES
        // ==========================================

        private void MostrarImagenPredeterminada()
        {
            MostrarFotoPerfilPredeterminada();

            MostrarVistaPreviaPredeterminada();
        }

        // ==========================================
        // RESTAURAR FOTOGRAFÍA GUARDADA
        // ==========================================

        private void RestaurarImagenAnterior()
        {
            MostrarFotografiaGuardada();
        }

        // ==========================================
        // ELEGIR IMAGEN
        // ==========================================

        private void ElegirImagen_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            OpenFileDialog dialogo =
                new OpenFileDialog
                {
                    Title =
                        "Seleccionar imagen de perfil",

                    Filter =
                        "Imágenes JPG y PNG (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",

                    Multiselect = false,

                    CheckFileExists = true
                };

            if (dialogo.ShowDialog() != true)
                return;

            try
            {
                FileInfo archivo =
                    new FileInfo(dialogo.FileName);

                // VALIDAR TAMAÑO
                if (archivo.Length == 0 ||
                    archivo.Length > 5 * 1024 * 1024)
                {
                    MostrarError(
                        "La imagen debe tener contenido y no superar los 5 MB.");

                    return;
                }

                // LEER IMAGEN
                byte[] contenido =
                    File.ReadAllBytes(
                        dialogo.FileName);

                // CONVERTIR Y MOSTRAR ANTES
                // DE CONFIRMAR LA SELECCIÓN
                BitmapImage imagen =
                    ConvertirImagen(contenido);

                // INVALIDAR DESCARGAS ANTERIORES
                versionImagen++;
                imagenOcultaTemporalmente = false;

                // GUARDAR RUTA SELECCIONADA
                rutaImagenSeleccionada =
                    dialogo.FileName;

                // MOSTRAR EN EL ENCABEZADO
                ImgPerfil.Source = imagen;

                // MOSTRAR EN VISTA PREVIA
                ImgVistaPrevia.Source = imagen;

                // HABILITAR BASURERO
                BtnEliminarImagen.IsEnabled = true;

                // LIMPIAR ERROR
                TxtError.Text = "";

                TxtError.Visibility =
                    Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MostrarError(
                    "No se pudo seleccionar la imagen: " +
                    ex.Message);
            }
        }

        // ==========================================
        // QUITAR IMAGEN SELECCIONADA
        // ==========================================


        private void EliminarImagen_Click(object sender, RoutedEventArgs e)
        {
            if (guardando)
                return;

            // Quitar únicamente del formulario, sin modificar SQL ni almacenamiento.
            rutaImagenSeleccionada = null;
            imagenOcultaTemporalmente = true;
            versionImagen++;

            MostrarFotoPerfilPredeterminada();
            MostrarVistaPreviaPredeterminada();
            BtnEliminarImagen.IsEnabled = false;

            TxtError.Text = "";
            TxtError.Visibility = Visibility.Collapsed;
        }

        // ==========================================
        // VALIDAR FORMULARIO
        // ==========================================

        private bool ValidarDatos()
        {
            var sexo = (CmbSexo.SelectedItem as ComboBoxItem)?.Tag as string;
            var resultado = new ValidadorPaciente().Validar(new EntradaPaciente(
                TxtIdentidad.Text.Trim(), TxtNombres.Text.Trim(), TxtApellidos.Text.Trim(),
                DpFechaNacimiento.SelectedDate?.ToString("yyyy/MM/dd") ?? "", sexo,
                TxtTelefono.Text.Trim()));
            resultado.Campo<string?>("Correo", TxtCorreo.Text.Trim())
                .Requerido("Ingresa un correo electrónico válido.")
                .CorreoValido("Ingresa un correo electrónico válido.");
            if (!resultado.EsValido)
            {
                MostrarError(resultado.Errores[0].Mensaje);
                return false;
            }
            return true;
        }

        // ==========================================
        // GUARDAR CAMBIOS
        // ==========================================

        private async void Guardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando || !ValidarDatos())
                return;

            if (imagenOcultaTemporalmente)
            {
                try
                {
                    await ClienteApi.EliminarMiImagenPerfilAsync();

                    fotografiaGuardada = null;
                    imagenOcultaTemporalmente = false;
                    MostrarImagenPredeterminada();
                }
                catch (Exception ex)
                {
                    MostrarError(
                        "Los datos personales se actualizaron, pero no se pudo " +
                        "quitar la fotografía del perfil: " + ex.Message);
                    return;
                }
            }
            guardando = true;

            // BLOQUEAR BOTONES DURANTE GUARDADO
            BtnGuardar.IsEnabled = false;

            BtnElegirImagen.IsEnabled = false;

            BtnEliminarImagen.IsEnabled = false;

            TxtError.Visibility =
                Visibility.Collapsed;

            try
            {
                // OBTENER SEXO
                string sexo =
                    (CmbSexo.SelectedItem as ComboBoxItem)?
                        .Tag?.ToString() ?? "";

                // DATOS PERSONALES
                var datos = new
                {
                    IdUsuario = (int?)null,

                    Identidad =
                        TxtIdentidad.Text.Trim(),

                    Nombres =
                        TxtNombres.Text.Trim(),

                    Apellidos =
                        TxtApellidos.Text.Trim(),

                    FechaNacimiento =
                        DpFechaNacimiento
                            .SelectedDate!
                            .Value.ToString("yyyy-MM-dd"),

                    Genero = sexo,

                    Telefono =
                        TxtTelefono.Text.Trim(),

                    CorreoContacto =
                        TxtCorreo.Text.Trim()
                };

                // ACTUALIZAR INFORMACIÓN EN LA API
                using HttpResponseMessage respuesta =
                    await ClienteApi
                        .PutAutenticadoAsync(
                            "api/auth/me/datos",
                            datos);

                // VERIFICAR RESPUESTA
                if (!respuesta.IsSuccessStatusCode)
                {
                    string detalle =
                        await respuesta.Content
                            .ReadAsStringAsync();

                    MostrarError(
                        "No se pudieron guardar los datos. " +
                        detalle);

                    return;
                }

                // ==================================
                // GUARDAR NUEVA FOTOGRAFÍA
                // ==================================

                if (!string.IsNullOrWhiteSpace(
                    rutaImagenSeleccionada))
                {
                    try
                    {
                        await ClienteApi
                            .SubirMiImagenPerfilAsync(
                                rutaImagenSeleccionada);

                        // LIMPIAR SELECCIÓN
                        rutaImagenSeleccionada = null;
                    }
                    catch (Exception ex)
                    {
                        MostrarError(
                            "Los datos se actualizaron, pero no se pudo " +
                            "guardar la fotografía: " +
                            ex.Message);

                        return;
                    }
                }

                // ==================================
                // REGRESAR A PERFILVIEW
                // ==================================

                Guardado?.Invoke();
            }
            catch (Exception ex)
            {
                MostrarError(
                    "No se pudo actualizar el perfil: " +
                    ex.Message);
            }
            finally
            {
                guardando = false;

                BtnGuardar.IsEnabled = true;

                BtnElegirImagen.IsEnabled = true;

                BtnEliminarImagen.IsEnabled =
                    !imagenOcultaTemporalmente && (rutaImagenSeleccionada != null || fotografiaGuardada != null);
            }
        }

        // ==========================================
        // CANCELAR
        // ==========================================

        private void Cancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            // REGRESAR SIN GUARDAR
            Cancelado?.Invoke();
        }

        // ==========================================
        // MOSTRAR ERROR
        // ==========================================

        private void MostrarError(string mensaje)
        {
            TxtError.Text = mensaje;

            TxtError.Visibility =
                Visibility.Visible;
        }
    }
}
