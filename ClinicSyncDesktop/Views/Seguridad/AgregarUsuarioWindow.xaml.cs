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

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.Windows.Window" />
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
public partial class AgregarUsuarioWindow : Window
{
    /// <summary>
    /// The usuarios service
    /// </summary>
    private readonly UsuariosApiService? _usuariosService;
    /// <summary>
    /// The roles service
    /// </summary>
    private readonly RolesApiService? _rolesService;
    /// <summary>
    /// The credenciales
    /// </summary>
    private bool _credenciales;
    /// <summary>
    /// The ruta imagen
    /// </summary>
    private string? _rutaImagen;
    /// <summary>
    /// The validador
    /// </summary>
    private readonly ValidadorUsuario _validador = new();
    /// <summary>
    /// The controles
    /// </summary>
    private readonly Dictionary<string, Control> _controles;

    /// <summary>
    /// Gets a value indicating whether [error al registrar].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [error al registrar]; otherwise, <c>false</c>.
    /// </value>
    public bool ErrorAlRegistrar { get; private set; }
    /// <summary>
    /// Gets the mensaje error.
    /// </summary>
    /// <value>
    /// The mensaje error.
    /// </value>
    public string MensajeError { get; private set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgregarUsuarioWindow"/> class.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AgregarUsuarioWindow"/> class.
    /// </summary>
    /// <param name="usuariosService">The usuarios service.</param>
    /// <param name="rolesService">The roles service.</param>
    public AgregarUsuarioWindow(UsuariosApiService usuariosService, RolesApiService rolesService) : this()
    {
        _usuariosService = usuariosService;
        _rolesService = rolesService;
        Loaded += AgregarUsuarioWindow_Loaded;
    }

    /// <summary>
    /// Handles the Loaded event of the AgregarUsuarioWindow control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Handles the Click event of the Siguiente control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Handles the Click event of the Regresar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Handles the Click event of the Guardar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Validars the datos personales.
    /// </summary>
    /// <returns></returns>
    private bool ValidarDatosPersonales()
    {
        var resultado = _validador.ValidarDatosPersonales(CrearEntrada());
        return HelperValidacion.AplicarResultado(resultado, _controles,
            mensaje => MostrarError(mensaje), LimpiarError);
    }

    /// <summary>
    /// Validars the credenciales.
    /// </summary>
    /// <returns></returns>
    private bool ValidarCredenciales()
    {
        var resultado = _validador.ValidarCredenciales(CrearEntrada());
        return HelperValidacion.AplicarResultado(resultado, _controles,
            mensaje => MostrarError(mensaje, true), LimpiarError);
    }

    /// <summary>
    /// Crears the entrada.
    /// </summary>
    /// <returns></returns>
    private EntradaUsuario CrearEntrada() => new(
        TxtIdentidad.Text.Trim(), TxtNombres.Text.Trim(), TxtApellidos.Text.Trim(),
        TxtTelefono.Text.Trim(), DpFechaNacimiento.SelectedDate, CmbGenero.SelectedItem,
        CmbRol.SelectedItem, TxtNombreUsuario.Text.Trim(), TxtCorreo.Text.Trim());

    /// <summary>
    /// Handles the Click event of the SeleccionarImagen control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Handles the Click event of the QuitarImagen control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void QuitarImagen_Click(object sender, RoutedEventArgs e)
    {
        TxtImagen.Clear();
        _rutaImagen = null;
        ((Image)FindName("ImgPerfil")).Source = null;
        BtnQuitarImagen.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Handles the Click event of the Cancelar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Handles the MouseLeftButtonDown event of the Encabezado control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    /// <summary>
    /// Mostrars the error.
    /// </summary>
    /// <param name="mensaje">The mensaje.</param>
    /// <param name="credenciales">if set to <c>true</c> [credenciales].</param>
    private void MostrarError(string mensaje, bool credenciales = false)
    {
        var control = credenciales ? TxtErrorCredenciales : TxtError;
        control.Text = mensaje;
        control.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Limpiars the error.
    /// </summary>
    private void LimpiarError()
    {
        TxtError.Visibility = Visibility.Collapsed;
        TxtErrorCredenciales.Visibility = Visibility.Collapsed;
    }
}
