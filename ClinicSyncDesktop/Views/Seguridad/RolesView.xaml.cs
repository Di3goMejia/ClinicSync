using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClinicSyncDesktop.Views;

public partial class RolesView : UserControl
{
    private readonly RolesApiService? _rolesService;
    private ICollectionView? _vista;
    private ObservableCollection<Rol> _roles = [];

    public RolesView()
    {
        InitializeComponent();
    }

    public RolesView(RolesApiService rolesService) : this()
    {
        _rolesService = rolesService;
        Loaded += RolesView_Loaded;
    }

    private async void RolesView_Loaded(object sender, RoutedEventArgs e)
    {
        await CargarRolesAsync();
    }

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
        return string.IsNullOrEmpty(texto)
            || rol.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase)
            || rol.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase);
    }

    private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
    {
        _vista?.Refresh();
    }

    private async void CmbEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
            await CargarRolesAsync();
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
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {

    }

    private void BtnOpciones_Click(object sender, RoutedEventArgs e)
    {

    }
}
