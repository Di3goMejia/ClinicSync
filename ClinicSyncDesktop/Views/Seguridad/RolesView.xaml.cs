using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Views.Seguridad;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClinicSyncDesktop.Views.Seguridad;

public partial class RolesView : UserControl
{
    private readonly RolesApiService? _rolesService;
    private ICollectionView? _vista;
    private ObservableCollection<Rol> _roles = [];

    public RolesView() => InitializeComponent();

    public RolesView(RolesApiService rolesService) : this()
    {
        _rolesService = rolesService;
        Loaded += RolesView_Loaded;
    }

    private async void RolesView_Loaded(object sender, RoutedEventArgs e) => await CargarRolesAsync();

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

    private bool FiltrarRol(object obj)
    {
        if (obj is not Rol rol)
            return false;
        var texto = TxtBuscar.Text.Trim();
        return string.IsNullOrEmpty(texto) ||
            rol.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
            rol.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase);
    }

    private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) => _vista?.Refresh();

    private async void CmbEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
            await CargarRolesAsync();
    }

    private async void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {
        if (_rolesService is null)
            return;
        var ventana = new AgregarRolWindow { Owner = Window.GetWindow(this) };
        if (ventana.ShowDialog() != true)
            return;
        try
        {
            await _rolesService.CrearAsync(
                ventana.Nombre,
                ventana.Descripcion,
                ventana.Permisos);
            await CargarRolesAsync();
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
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
            await _rolesService.ModificarAsync(
                rol.IdRol,
                ventana.Nombre,
                ventana.Descripcion,
                ventana.Permisos);

            if (rol.Activo != ventana.Activo)
                await _rolesService.CambiarEstadoAsync(rol.IdRol, ventana.Activo);
            await CargarRolesAsync();
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    private async void BtnCambiarEstado_Click(object sender, RoutedEventArgs e)
    {
        if (_rolesService is null || sender is not Button { Tag: Rol rol })
            return;
        try
        {
            await _rolesService.CambiarEstadoAsync(rol.IdRol, !rol.Activo);
            await CargarRolesAsync();
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
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
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }
}