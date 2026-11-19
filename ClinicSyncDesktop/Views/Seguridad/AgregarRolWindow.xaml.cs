using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSyncDesktop.Views.Seguridad;

public partial class AgregarRolWindow : Window
{
    private readonly ValidadorRol _validador = new();
    private readonly Dictionary<string, Control> _controles;
    private readonly ObservableCollection<PermisoFilaDesktop> _permisos = [];
    private readonly PermisosApiService _permisosService = new();
    private readonly RolesApiService _rolesService = new();
    public string Nombre => TxtNombre.Text.Trim();
    public string? Descripcion => string.IsNullOrWhiteSpace(TxtDescripcion.Text) ? null : TxtDescripcion.Text.Trim();
    public IReadOnlyList<string> Permisos => _permisos.Where(p => p.Seleccionado).Select(p => p.Codigo).ToList();

    public AgregarRolWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await CargarAsync();

        _controles = new Dictionary<string, Control>
        {
            [nameof(EntradaRol.Nombre)] = TxtNombre,
            [nameof(EntradaRol.Descripcion)] = TxtDescripcion
        };
    }

    private async Task CargarAsync()
    {
        try
        {
            CmbRolBase.ItemsSource = await _rolesService.ListarAsync(activo: true);

            foreach (var permiso in await _permisosService.ListarAsync())
                _permisos.Add(new PermisoFilaDesktop(permiso.Codigo, permiso.Modulo, permiso.Nombre));
            
            CmbModulo.ItemsSource = new[] { "Todos" }.Concat(_permisos.Select(p => p.Modulo).Distinct().Order()).ToList();
            ListaPermisos.ItemsSource = _permisos;
        }
        catch (Exception ex) { MostrarError(ex.Message); }
    }

    private bool ValidarInfo()
    {
        var entrada = new EntradaRol(
            Nombre: TxtNombre.Text.Trim(),
            Descripcion: TxtDescripcion.Text.Trim()
        );

        var resultado = _validador.Validar(entrada);
        return HelperValidacion.AplicarResultado(resultado, _controles, MostrarError, () => { });
    }

    private void Siguiente_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarInfo()) return;
        PanelInfo.Visibility = Visibility.Collapsed;
        PanelPermisos.Visibility = Visibility.Visible;
        BtnPermisos.IsEnabled = true;
        BtnSiguiente.Visibility = Visibility.Collapsed;
        BtnGuardar.Visibility = Visibility.Visible;
        TxtMensajePermisos.Text = $"Selecciona los permisos que desea asignar al rol {Nombre}";
    }

    private void Info_Click(object sender, RoutedEventArgs e) { PanelInfo.Visibility = Visibility.Visible; PanelPermisos.Visibility = Visibility.Collapsed; BtnSiguiente.Visibility = Visibility.Visible; BtnGuardar.Visibility = Visibility.Collapsed; }
    private void Permisos_Click(object sender, RoutedEventArgs e) { if (!ValidarInfo()) return; PanelInfo.Visibility = Visibility.Collapsed; PanelPermisos.Visibility = Visibility.Visible; BtnSiguiente.Visibility = Visibility.Collapsed; BtnGuardar.Visibility = Visibility.Visible; }
    private void Info_TextChanged(object sender, TextChangedEventArgs e) => BtnPermisos.IsEnabled = !string.IsNullOrWhiteSpace(TxtNombre.Text);
    private void RolBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbRolBase.SelectedItem is not Rol rol || _permisos.Count == 0) return;
        foreach (var permiso in _permisos)
            permiso.Seleccionado = false;
        _ = CopiarPermisosAsync(rol.IdRol);
    }
    private async Task CopiarPermisosAsync(int idRol)
    {
        try
        {
            var detalle = await _rolesService.ObtenerAsync(idRol);
            var seleccionados = detalle.Permisos.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var permiso in _permisos)
                permiso.Seleccionado = seleccionados.Contains(permiso.Codigo);
            ListaPermisos.Items.Refresh();
        }
        catch (Exception ex) { MostrarError(ex.Message); }
    }
    private void FiltroPermisos_TextChanged(object sender, TextChangedEventArgs e) => FiltrarPermisos();
    private void FiltroPermisos_Changed(object sender, SelectionChangedEventArgs e) => FiltrarPermisos();
    private void FiltrarPermisos()
    {
        var texto = TxtBuscarPermiso.Text.Trim();
        var modulo = CmbModulo.SelectedItem as string;
        ListaPermisos.ItemsSource = _permisos.Where(p => (string.IsNullOrEmpty(texto) || p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) || p.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase)) && (modulo is null || modulo == "Todos" || p.Modulo == modulo)).ToList();
    }
    private void Guardar_Click(object sender, RoutedEventArgs e) { if (ValidarInfo()) DialogResult = true; }
    private void MostrarError(string mensaje) { TxtError.Text = mensaje; TxtError.Visibility = Visibility.Visible; }
    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
}
