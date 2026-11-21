using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using ClinicSyncDesktop.Views;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views.Seguridad;

public partial class AgregarUsuarioWindow : Window
{
    private readonly UsuariosApiService? _usuariosService;
    private readonly RolesApiService? _rolesService;
    private bool _credenciales;
    private string? _rutaImagen;
    private readonly ValidadorUsuario _validador = new();
    private readonly Dictionary<string, Control> _controles;

    public bool ErrorAlRegistrar { get; private set; }
    public string MensajeError { get; private set; } = string.Empty;

    public AgregarUsuarioWindow()
    {
        InitializeComponent();
        _controles = new Dictionary<string, Control>
        {
            [nameof(EntradaUsuario.Identidad)] = TxtIdentidad,
            [nameof(EntradaUsuario.Nombres)] = TxtNombres,
            [nameof(EntradaUsuario.Apellidos)] = TxtApellidos,
            [nameof(EntradaUsuario.Telefono)] = TxtTelefono,
            [nameof(EntradaUsuario.FechaNacimiento)] = DpFechaNacimiento,
            [nameof(EntradaUsuario.Genero)] = CmbGenero,
            [nameof(EntradaUsuario.Rol)] = CmbRol,
            [nameof(EntradaUsuario.NombreUsuario)] = TxtNombreUsuario,
            [nameof(EntradaUsuario.Correo)] = TxtCorreo
        };

        TxtIdentidad.TextChanged += (_, _) => LimpiarError();
        TxtNombres.TextChanged += (_, _) => LimpiarError();
        TxtApellidos.TextChanged += (_, _) => LimpiarError();
        TxtTelefono.TextChanged += (_, _) => LimpiarError();
        DpFechaNacimiento.SelectedDateChanged += (_, _) => LimpiarError();
        CmbGenero.SelectionChanged += (_, _) => LimpiarError();
        CmbRol.SelectionChanged += (_, _) => LimpiarError();
        TxtNombreUsuario.TextChanged += (_, _) => LimpiarError();
        TxtCorreo.TextChanged += (_, _) => LimpiarError();
    }

    public AgregarUsuarioWindow(UsuariosApiService usuariosService, RolesApiService rolesService) : this()
    {
        _usuariosService = usuariosService;
        _rolesService = rolesService;
        Loaded += AgregarUsuarioWindow_Loaded;
    }

    private async void AgregarUsuarioWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var roles = await _rolesService!.ListarAsync(activo: true);

            CmbRol.SelectedValuePath = nameof(Rol.IdRol);
            CmbRol.ItemsSource = roles;
        }
        catch (Exception ex)
        {
            MostrarError("No fue posible cargar los roles disponibles.");
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private void Siguiente_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarDatosPersonales())
            return;

        _credenciales = true;
        PanelDatosPersonales.Visibility = Visibility.Collapsed;
        PanelCredenciales.Visibility = Visibility.Visible;
        BarraPaso1.Background = (Brush)FindResource("Turquesa");
        BarraPaso2.Background = (Brush)FindResource("Turquesa");
        BtnSiguiente.Visibility = Visibility.Collapsed;
        BtnAgregar.Visibility = Visibility.Visible;
        BtnRegresar.Visibility = Visibility.Visible;
    }

    private void Regresar_Click(object sender, RoutedEventArgs e)
    {
        _credenciales = false;
        PanelDatosPersonales.Visibility = Visibility.Visible;
        PanelCredenciales.Visibility = Visibility.Collapsed;
        BarraPaso2.Background = new SolidColorBrush(Color.FromRgb(217, 221, 216));
        BtnSiguiente.Visibility = Visibility.Visible;
        BtnAgregar.Visibility = Visibility.Collapsed;
        BtnRegresar.Visibility = Visibility.Collapsed;
        LimpiarError();
    }

    private async void Guardar_Click(object sender, RoutedEventArgs e)
    {
        if (_usuariosService is null || !_credenciales || !ValidarCredenciales())
            return;

        try
        {
            var genero = ((ComboBoxItem)CmbGenero.SelectedItem).Tag.ToString()![0];
            var rol = (Rol)CmbRol.SelectedItem;
            var creado = await _usuariosService.CrearAsync(
                TxtIdentidad.Text.Trim(), TxtNombres.Text.Trim(), TxtApellidos.Text.Trim(),
                DateOnly.FromDateTime(DpFechaNacimiento.SelectedDate!.Value), genero, TxtTelefono.Text.Trim(), rol.IdRol,
                TxtNombreUsuario.Text.Trim(), TxtCorreo.Text.Trim());

            if (!string.IsNullOrWhiteSpace(_rutaImagen))
                await _usuariosService.SubirImagenPerfilAsync(creado.IdUsuario, _rutaImagen);

            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorAlRegistrar = true;
            MensajeError = ex.Message;
            MostrarError(MensajeError, true);
        }
    }

    private bool ValidarDatosPersonales()
    {
        var resultado = _validador.ValidarDatosPersonales(CrearEntrada());
        return HelperValidacion.AplicarResultado(resultado, _controles,
            mensaje => MostrarError(mensaje), LimpiarError);
    }

    private bool ValidarCredenciales()
    {
        var resultado = _validador.ValidarCredenciales(CrearEntrada());
        return HelperValidacion.AplicarResultado(resultado, _controles,
            mensaje => MostrarError(mensaje, true), LimpiarError);
    }

    private EntradaUsuario CrearEntrada() => new(
        TxtIdentidad.Text.Trim(), TxtNombres.Text.Trim(), TxtApellidos.Text.Trim(),
        TxtTelefono.Text.Trim(), DpFechaNacimiento.SelectedDate, CmbGenero.SelectedItem,
        CmbRol.SelectedItem, TxtNombreUsuario.Text.Trim(), TxtCorreo.Text.Trim());

    private void SeleccionarImagen_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Filter = "Imágenes (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
            Title = "Seleccionar imagen de perfil"
        };

        if (dialogo.ShowDialog() != true)
            return;

        if (new FileInfo(dialogo.FileName).Length > 5 * 1024 * 1024)
        {
            MostrarError("La imagen no puede superar los 5 MB.");
            return;
        }

        TxtImagen.Text = dialogo.FileName;
        _rutaImagen = dialogo.FileName;
        ((Image)FindName("ImgPerfil")).Source = new BitmapImage(new Uri(dialogo.FileName));
        BtnQuitarImagen.Visibility = Visibility.Visible;
    }

    private void QuitarImagen_Click(object sender, RoutedEventArgs e)
    {
        TxtImagen.Clear();
        _rutaImagen = null;
        ((Image)FindName("ImgPerfil")).Source = null;
        BtnQuitarImagen.Visibility = Visibility.Collapsed;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        bool hayDatos = !string.IsNullOrWhiteSpace(TxtIdentidad.Text) ||
            !string.IsNullOrWhiteSpace(TxtNombres.Text) ||
            !string.IsNullOrWhiteSpace(TxtApellidos.Text) ||
            !string.IsNullOrWhiteSpace(TxtTelefono.Text) ||
            DpFechaNacimiento.SelectedDate.HasValue ||
            !string.IsNullOrWhiteSpace(TxtNombreUsuario.Text) ||
            !string.IsNullOrWhiteSpace(TxtCorreo.Text);

        if (hayDatos)
        {
            var descarte = new ConfirmarDescarteWindow("¿Seguro que desea descartar este registro?") { Owner = this };
            if (descarte.ShowDialog() != true)
                return;
        }

        Close();
    }

    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void MostrarError(string mensaje, bool credenciales = false)
    {
        var control = credenciales ? TxtErrorCredenciales : TxtError;
        control.Text = mensaje;
        control.Visibility = Visibility.Visible;
    }

    private void LimpiarError()
    {
        TxtError.Visibility = Visibility.Collapsed;
        TxtErrorCredenciales.Visibility = Visibility.Collapsed;
    }
}
