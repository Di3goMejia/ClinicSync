using ClinicSyncDesktop.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views.Seguridad;

public partial class EditarUsuarioWindow : Window
{
    private readonly UsuariosApiService _usuarios;
    private readonly RolesApiService _roles;
    private readonly UsuarioDetalle _original;
    private bool _guardando;

    public bool ErrorAlGuardar { get; private set; }
    public string MensajeError { get; private set; } = "No fue posible actualizar el usuario.";

    public EditarUsuarioWindow(UsuarioDetalle usuario, UsuariosApiService usuarios, RolesApiService roles)
    {
        InitializeComponent();
        _original = usuario;
        _usuarios = usuarios;
        _roles = roles;
        CargarDatos();

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
                }
                catch { }
            }
        };
    }

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

    private void Datos_Click(object sender, RoutedEventArgs e)
    {
        PanelDatos.Visibility = Visibility.Visible;
        PanelCuenta.Visibility = Visibility.Collapsed;
    }

    private void Cuenta_Click(object sender, RoutedEventArgs e)
    {
        PanelDatos.Visibility = Visibility.Collapsed;
        PanelCuenta.Visibility = Visibility.Visible;
    }

    private bool Validar()
    {
        TxtError.Visibility = Visibility.Collapsed;
        if (string.IsNullOrWhiteSpace(TxtIdentidad.Text))
            return Error("La identidad es obligatoria.", TxtIdentidad);

        if (string.IsNullOrWhiteSpace(TxtNombres.Text))
            return Error("Los nombres son obligatorios.", TxtNombres);

        if (string.IsNullOrWhiteSpace(TxtApellidos.Text))
            return Error("Los apellidos son obligatorios.", TxtApellidos);

        if (DpFechaNacimiento.SelectedDate is null)
            return Error("La fecha de nacimiento es obligatoria.", DpFechaNacimiento);

        if (CmbGenero.SelectedItem is null)
            return Error("Selecciona el género del usuario.", CmbGenero);

        if (string.IsNullOrWhiteSpace(TxtTelefono.Text))
            return Error("El teléfono es obligatorio.", TxtTelefono);

        if (CmbRol.SelectedItem is not Rol)
            return Error("Selecciona el rol del usuario.", CmbRol);

        return true;
    }

    private bool Error(string mensaje, UIElement campo)
    {
        TxtError.Text = mensaje;
        TxtError.Visibility = Visibility.Visible;
        campo.Focus();

        return false;
    }

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
            TxtError.Text = MensajeError;
            TxtError.Visibility = Visibility.Visible;
        }
        finally
        {
            _guardando = false;
        }
    }

    private async void SeleccionarImagen_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog { Filter = "Imágenes (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png" };
        if (dialogo.ShowDialog() != true) return;
        if (new FileInfo(dialogo.FileName).Length > 5 * 1024 * 1024)
        {
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
        }
        catch (Exception ex) { TxtError.Text = ex.Message; TxtError.Visibility = Visibility.Visible; }
    }

    private async void Reenviar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _usuarios.ReenviarActivacionAsync(_original.IdUsuario);
            MessageBox.Show("La solicitud de activación fue reenviada.", "Cuenta", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { TxtError.Text = ex.Message; TxtError.Visibility = Visibility.Visible; }
    }

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

            MessageBox.Show(inactivo ? "El usuario fue reactivado." : "El usuario fue desactivado.", "Cuenta", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) 
        { 
            TxtError.Text = ex.Message; 
            TxtError.Visibility = Visibility.Visible; 
        }
    }

    private async void Restablecer_Click(object sender, RoutedEventArgs e)
    {
        var password = SolicitarPassword("Confirma la solicitud ingresando tu contraseña:");
        if (password is null) return;
        
        try 
        { 
            await _usuarios.RestablecerPasswordAsync(_original.IdUsuario, password); 
            MessageBox.Show("La solicitud fue enviada al usuario.", "Cuenta", MessageBoxButton.OK, MessageBoxImage.Information); 
        }
        catch (Exception ex) 
        { 
            TxtError.Text = ex.Message; 
            TxtError.Visibility = Visibility.Visible; 
        }
    }

    private string? SolicitarPassword(string mensaje)
    {
        var dialogo = new SolicitarPasswordWindow(mensaje) { Owner = this };
        return dialogo.ShowDialog() == true ? dialogo.Password : null;
    }

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

    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
    { 
        if (e.LeftButton == MouseButtonState.Pressed) DragMove(); 
    }
}
