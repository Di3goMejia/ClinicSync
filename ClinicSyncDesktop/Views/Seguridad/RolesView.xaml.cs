using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Views;
using ClinicSyncDesktop.Views.Seguridad;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClinicSyncDesktop.Views.Seguridad;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.Windows.Controls.UserControl" />
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
/// <seealso cref="System.Windows.Markup.IStyleConnector" />
public partial class RolesView : UserControl
{
    /// <summary>
    /// The roles service
    /// </summary>
    private readonly RolesApiService? _rolesService;
    /// <summary>
    /// The vista
    /// </summary>
    private ICollectionView? _vista;
    /// <summary>
    /// The roles
    /// </summary>
    private ObservableCollection<Rol> _roles = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="RolesView"/> class.
    /// </summary>
    public RolesView() => InitializeComponent();

    /// <summary>
    /// Initializes a new instance of the <see cref="RolesView"/> class.
    /// </summary>
    /// <param name="rolesService">The roles service.</param>
    public RolesView(RolesApiService rolesService) : this()
    {
        _rolesService = rolesService;
        Loaded += RolesView_Loaded;
    }

    /// <summary>
    /// Handles the Loaded event of the RolesView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void RolesView_Loaded(object sender, RoutedEventArgs e) => await CargarRolesAsync();

    /// <summary>
    /// Cargars the roles asynchronous.
    /// </summary>
    private async Task CargarRolesAsync()
    {
        if (_rolesService is null)
            return;

        try
        {
            bool? activo = CmbEstado.SelectedIndex switch
            {
                1 => true,
                2 => false,
                _ => null
            };
            var roles = await _rolesService.ListarAsync(TxtBuscar.Text, activo);
            _roles = new ObservableCollection<Rol>(roles);
            _vista = CollectionViewSource.GetDefaultView(_roles);
            _vista.Filter = FiltrarRol;
            dgRoles.ItemsSource = _vista;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

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
    /// Filtrars the rol.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns></returns>
    private bool FiltrarRol(object obj)
    {
        if (obj is not Rol rol)
            return false;
        var texto = TxtBuscar.Text.Trim();
        return string.IsNullOrEmpty(texto) ||
            rol.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
            rol.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Handles the TextChanged event of the TxtBuscar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
    private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) => _vista?.Refresh();

    /// <summary>
    /// Handles the SelectionChanged event of the CmbEstado control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
    private async void CmbEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
            await CargarRolesAsync();
    }

    /// <summary>
    /// Handles the Click event of the BtnAgregar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {
        if (_rolesService is null)
            return;
        var ventana = new AgregarRolWindow { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() != true)
            return;
        try
        {
            await _rolesService.CrearAsync(ventana.Nombre, ventana.Descripcion, ventana.Permisos);
            await CargarRolesAsync();
            MostrarAviso("Rol registrado exitosamente.", exitoso: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            MostrarAviso(string.IsNullOrWhiteSpace(ex.Message) ? "No fue posible registrar el rol." : ex.Message, exitoso: false);
        }
    }

    private async void BtnEditar_Click(object sender, RoutedEventArgs e)
    {
        if (_rolesService is null || sender is not Button { Tag: Rol rol })
            return;
        var ventana = new EditarRolWindow(rol) { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() != true)
            return;
        try
        {
            await _rolesService.ModificarAsync(rol.IdRol, ventana.Nombre, ventana.Descripcion, ventana.Permisos);
            if (rol.Activo != ventana.Activo)
                await _rolesService.CambiarEstadoAsync(rol.IdRol, ventana.Activo);
            await CargarRolesAsync();
            MostrarAviso("Rol actualizado correctamente.", exitoso: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            MostrarAviso(string.IsNullOrWhiteSpace(ex.Message) ? "No fue posible actualizar el rol." : ex.Message, exitoso: false);
        }
    }

    private async void BtnCambiarEstado_Click(object sender, RoutedEventArgs e)
    {
        if (_rolesService is null || sender is not Button { Tag: Rol rol })
            return;
        try
        {
            await _rolesService.CambiarEstadoAsync(rol.IdRol, !rol.Activo);
            await CargarRolesAsync();
            MostrarAviso(rol.Activo ? "Rol desactivado." : "Rol activado.", exitoso: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            MostrarAviso(string.IsNullOrWhiteSpace(ex.Message) ? "No fue posible cambiar el estado del rol." : ex.Message, exitoso: false);
        }
    }

    private async void BtnOpciones_Click(object sender, RoutedEventArgs e)
    {
        if (_rolesService is null || sender is not Button boton || boton.DataContext is not Rol rol)
            return;

        var ventana = new EditarRolWindow(rol) { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() != true)
            return;

        try
        {
            await _rolesService.ModificarAsync(rol.IdRol, ventana.Nombre, ventana.Descripcion, ventana.Permisos);
            if (rol.Activo != ventana.Activo)
                await _rolesService.CambiarEstadoAsync(rol.IdRol, ventana.Activo);
            await CargarRolesAsync();
            MostrarAviso("Rol actualizado correctamente.", exitoso: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            MostrarAviso(string.IsNullOrWhiteSpace(ex.Message) ? "No fue posible actualizar el rol." : ex.Message, exitoso: false);
        }
    }
}