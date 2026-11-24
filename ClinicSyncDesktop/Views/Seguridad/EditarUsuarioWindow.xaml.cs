using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace ClinicSyncDesktop.Views.Seguridad;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.Windows.Window" />
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
public partial class EditarUsuarioWindow : Window
{
    /// <summary>
    /// The usuarios
    /// </summary>
    private readonly UsuariosApiService _usuarios;
    /// <summary>
    /// The roles
    /// </summary>
    private readonly RolesApiService _roles;
    /// <summary>
    /// The original
    /// </summary>
    private readonly UsuarioDetalle _original;
    /// <summary>
    /// The guardando
    /// </summary>
    private bool _guardando;

    /// <summary>
    /// Gets a value indicating whether [error al guardar].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [error al guardar]; otherwise, <c>false</c>.
    /// </value>
    public bool ErrorAlGuardar { get; private set; }
    /// <summary>
    /// Gets the mensaje error.
    /// </summary>
    /// <value>
    /// The mensaje error.
    /// </value>
    public string MensajeError { get; private set; } = "No fue posible actualizar el usuario.";

    /// <summary>
    /// Initializes a new instance of the <see cref="EditarUsuarioWindow"/> class.
    /// </summary>
    /// <param name="usuario">The usuario.</param>
    /// <param name="usuarios">The usuarios.</param>
    /// <param name="roles">The roles.</param>
    public EditarUsuarioWindow(UsuarioDetalle usuario, UsuariosApiService usuarios, RolesApiService roles)
    {
        InitializeComponent();
        _original = usuario;
        _usuarios = usuarios;
        _roles = roles;
        CargarDatos();

        TxtIdentidad.TextChanged += (_, _) => LimpiarError();
        TxtNombres.TextChanged += (_, _) => LimpiarError();
        TxtApellidos.TextChanged += (_, _) => LimpiarError();
        TxtTelefono.TextChanged += (_, _) => LimpiarError();
        DpFechaNacimiento.SelectedDateChanged += (_, _) => LimpiarError();
        CmbGenero.SelectionChanged += (_, _) => LimpiarError();
        CmbRol.SelectionChanged += (_, _) => LimpiarError();

        Loaded += async (_, _) =>
        {
            CmbRol.ItemsSource = await _roles.ListarAsync(activo: true);
            CmbRol.SelectedValue = _original.IdRol;
            if (!string.IsNullOrWhiteSpace(_original.ImagenPerfil))
            {
                try
                {
                    var bytes = await _usuarios.DescargarImagenPerfilAsync(_original.IdUsuario);
                    using var stream = new MemoryStream(bytes);
                    var imagen = new BitmapImage();
                    imagen.BeginInit();
                    imagen.CacheOption = BitmapCacheOption.OnLoad;
                    imagen.StreamSource = stream;
                    imagen.EndInit();
                    ((Image)FindName("ImgPerfil")).Source = imagen;
                    TxtImagen.Text = _original.ImagenPerfil;
                    BtnQuitarImagen.Visibility = Visibility.Visible;
                }
                catch { }
            }
        };
    }

    /// <summary>
    /// Cargars the datos.
    /// </summary>
    private void CargarDatos()
    {
        TxtIdentidad.Text = _original.Identidad;
        TxtNombres.Text = _original.Nombres;
        TxtApellidos.Text = _original.Apellidos;
        DpFechaNacimiento.SelectedDate = _original.FechaNacimiento.ToDateTime(TimeOnly.MinValue);
        TxtTelefono.Text = _original.Telefono;
        TxtNombreUsuario.Text = _original.NombreUsuario;
        TxtCorreo.Text = _original.CorreoLogin;
        TxtEstado.Text = _original.Estado;
        BtnReenviar.Visibility = _original.Estado.Equals("PENDIENTE", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
        bool inactivo = _original.Estado.Equals("INACTIVO", StringComparison.OrdinalIgnoreCase)
            || _original.Estado.Equals("DESACTIVADO", StringComparison.OrdinalIgnoreCase);
        BtnCambiarEstado.Content = inactivo ? "Reactivar usuario" : "Desactivar usuario";
        CmbGenero.SelectedIndex = _original.Genero == 'F' ? 0 : 1;
    }

    /// <summary>
    /// Handles the Click event of the Datos control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void Datos_Click(object sender, RoutedEventArgs e)
    {
        PanelDatos.Visibility = Visibility.Visible;
        PanelCuenta.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Handles the Click event of the Cuenta control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void Cuenta_Click(object sender, RoutedEventArgs e)
    {
        PanelDatos.Visibility = Visibility.Collapsed;
        PanelCuenta.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Limpiars the error.
    /// </summary>
    private void LimpiarError()
    {
        TxtError.Text = string.Empty;
        TxtError.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Validars this instance.
    /// </summary>
    /// <returns></returns>
    private bool Validar()
    {
        LimpiarError();
        var resultado = new ValidadorUsuario().ValidarDatosPersonales(new EntradaUsuario(
            TxtIdentidad.Text.Trim(), TxtNombres.Text.Trim(), TxtApellidos.Text.Trim(),
            TxtTelefono.Text.Trim(), DpFechaNacimiento.SelectedDate,
            CmbGenero.SelectedItem, CmbRol.SelectedItem, "válido", "valido@clinic.local"));
        if (!HelperValidacion.AplicarResultado(resultado,
            new Dictionary<string, Control>
            {
                [nameof(EntradaUsuario.Identidad)] = TxtIdentidad,
                [nameof(EntradaUsuario.Nombres)] = TxtNombres,
                [nameof(EntradaUsuario.Apellidos)] = TxtApellidos,
                [nameof(EntradaUsuario.Telefono)] = TxtTelefono,
                [nameof(EntradaUsuario.FechaNacimiento)] = DpFechaNacimiento,
                [nameof(EntradaUsuario.Genero)] = CmbGenero,
                [nameof(EntradaUsuario.Rol)] = CmbRol
            }, mensaje => Error(mensaje, TxtIdentidad), LimpiarError))
            return false;
        return true;
    }

    /// <summary>
    /// Errors the specified mensaje.
    /// </summary>
    /// <param name="mensaje">The mensaje.</param>
    /// <param name="campo">The campo.</param>
    /// <returns></returns>
    private bool Error(string mensaje, UIElement campo)
    {
        TxtError.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xB3, 0x26, 0x1E));
        TxtError.Text = mensaje;
        TxtError.Visibility = Visibility.Visible;
        campo.Focus();

        return false;
    }

    /// <summary>
    /// Handles the Click event of the Guardar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void Guardar_Click(object sender, RoutedEventArgs e)
    {
        if (_guardando || !Validar())
            return;

        try
        {
            _guardando = true;

            await _usuarios.ModificarAsync(_original.IdUsuario, new
            {
                identidad = TxtIdentidad.Text.Trim(),
                nombres = TxtNombres.Text.Trim(),
                apellidos = TxtApellidos.Text.Trim(),
                fechaNacimiento = DateOnly.FromDateTime(DpFechaNacimiento.SelectedDate!.Value),
                genero = ((ComboBoxItem)CmbGenero.SelectedItem).Tag.ToString()![0],
                telefono = TxtTelefono.Text.Trim(),
                correoContacto = _original.CorreoContacto
            });

            var rol = (Rol)CmbRol.SelectedItem;
            if (_original.IdRol != rol.IdRol)
                await _usuarios.ModificarRolAsync(_original.IdUsuario, rol.IdRol);

            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorAlGuardar = true;
            MensajeError = ex.Message;
            TxtError.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xB3, 0x26, 0x1E));
            TxtError.Text = MensajeError;
            TxtError.Visibility = Visibility.Visible;
        }
        finally
        {
            _guardando = false;
        }
    }

    /// <summary>
    /// Handles the Click event of the SeleccionarImagen control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void SeleccionarImagen_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog { Filter = "Imágenes (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png" };
        if (dialogo.ShowDialog() != true) return;
        if (new FileInfo(dialogo.FileName).Length > 5 * 1024 * 1024)
        {
            TxtError.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xB3, 0x26, 0x1E));
            TxtError.Text = "La imagen no puede superar los 5 MB.";
            TxtError.Visibility = Visibility.Visible;
            return;
        }
        try
        {
            await _usuarios.SubirImagenPerfilAsync(_original.IdUsuario, dialogo.FileName);
            using var stream = File.OpenRead(dialogo.FileName);
            var imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.CacheOption = BitmapCacheOption.OnLoad;
            imagen.StreamSource = stream;
            imagen.EndInit();
            ((Image)FindName("ImgPerfil")).Source = imagen;
            TxtImagen.Text = dialogo.FileName;
            BtnQuitarImagen.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            TxtError.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xB3, 0x26, 0x1E));
            TxtError.Text = ex.Message;
            TxtError.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Handles the Click event of the QuitarImagen control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void QuitarImagen_Click(object sender, RoutedEventArgs e)
    {
        TxtImagen.Text = string.Empty;
        ((Image)FindName("ImgPerfil")).Source = null;
        BtnQuitarImagen.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Handles the Click event of the Reenviar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void Reenviar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _usuarios.ReenviarActivacionAsync(_original.IdUsuario);
            MostrarConfirmacion("La solicitud de activación fue reenviada.");
        }
        catch (Exception ex)
        {
            TxtError.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xB3, 0x26, 0x1E));
            TxtError.Text = ex.Message;
            TxtError.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Handles the Click event of the CambiarEstado control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void CambiarEstado_Click(object sender, RoutedEventArgs e)
    {
        bool inactivo = _original.Estado.Equals("INACTIVO", StringComparison.OrdinalIgnoreCase)
            || _original.Estado.Equals("DESACTIVADO", StringComparison.OrdinalIgnoreCase);
        var password = SolicitarPassword(inactivo
            ? "Confirma la reactivación ingresando tu contraseña:"
            : "Confirma la desactivación ingresando tu contraseña:");

        if (password is null) return;
        
        try
        {
            if (inactivo)
            {
                await _usuarios.ReactivarAsync(_original.IdUsuario, password);
                TxtEstado.Text = "ACTIVO";
                BtnCambiarEstado.Content = "Desactivar usuario";
            }
            else
            {
                await _usuarios.DesactivarAsync(_original.IdUsuario, password);
                TxtEstado.Text = "INACTIVO";
                BtnCambiarEstado.Content = "Reactivar usuario";
            }

            MostrarConfirmacion(inactivo ? "El usuario fue reactivado." : "El usuario fue desactivado.");
        }
        catch (Exception ex)
        {
            TxtError.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xB3, 0x26, 0x1E));
            TxtError.Text = ex.Message;
            TxtError.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Handles the Click event of the Restablecer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void Restablecer_Click(object sender, RoutedEventArgs e)
    {
        var password = SolicitarPassword("Confirma la solicitud ingresando tu contraseña:");
        if (password is null) return;
        
        try 
        { 
            await _usuarios.RestablecerPasswordAsync(_original.IdUsuario, password); 
            MostrarConfirmacion("La solicitud fue enviada al usuario.");
        }
        catch (Exception ex)
        {
            TxtError.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xB3, 0x26, 0x1E));
            TxtError.Text = ex.Message;
            TxtError.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Solicitars the password.
    /// </summary>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    private string? SolicitarPassword(string mensaje)
    {
        var dialogo = new SolicitarPasswordWindow(mensaje) { Owner = this };
        return dialogo.ShowDialog() == true ? dialogo.Password : null;
    }

    /// <summary>
    /// Handles the Click event of the Cerrar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        bool cambios = TxtIdentidad.Text != _original.Identidad ||
            TxtNombres.Text != _original.Nombres || 
            TxtApellidos.Text != _original.Apellidos || 
            TxtTelefono.Text != _original.Telefono || 
            DpFechaNacimiento.SelectedDate?.Date != _original.FechaNacimiento.ToDateTime(TimeOnly.MinValue).Date;

        if (cambios)
        {
            var descarte = new ConfirmarDescarteWindow("¿Seguro que desea descartar esta edición?") { Owner = this };
            if (descarte.ShowDialog() != true)
                return;
        }

        DialogResult = false;
    }

    /// <summary>
    /// Handles the MouseLeftButtonDown event of the Encabezado control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
    { 
        if (e.LeftButton == MouseButtonState.Pressed) DragMove(); 
    }

    /// <summary>
    /// Mostrars the confirmacion.
    /// </summary>
    /// <param name="mensaje">The mensaje.</param>
    private void MostrarConfirmacion(string mensaje)
    {
        TxtError.Text = mensaje;
        TxtError.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x14, 0x8D, 0xAA));
        TxtError.Visibility = Visibility.Visible;
    }
}
