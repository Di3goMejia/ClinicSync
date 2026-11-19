using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClinicSyncDesktop.Views.Seguridad;

public partial class EditarRolWindow : Window
{
    private readonly ValidadorRol _validador = new();
    private readonly Dictionary<string, Control> _controles;
    private readonly Rol? _rol;
    private readonly ObservableCollection<PermisoFilaDesktop> _permisos = [];
    private readonly PermisosApiService _permisosService = new();
    private readonly RolesApiService _rolesService = new();
    public bool EsEdicion => _rol is not null;
    public string Nombre => 
        TxtNombre.Text.Trim();
    public string? Descripcion => 
        string.IsNullOrWhiteSpace(TxtDescripcion.Text) ? null : TxtDescripcion.Text.Trim();
    public bool Activo => 
        ChkActivo.IsChecked == true;
    public IReadOnlyList<string> Permisos => 
        _permisos.Where(p => p.Seleccionado).Select(p => p.Codigo).ToList();
    
    public int? IdRolBase => (CmbRolBase.SelectedItem as Rol)?.IdRol;

    public EditarRolWindow(Rol? rol = null)
    {
        InitializeComponent();

        _controles = new Dictionary<string, Control>
        {
            [nameof(EntradaRol.Nombre)] = TxtNombre,
            [nameof(EntradaRol.Descripcion)] = TxtDescripcion
        };

        _rol = rol;
        if (rol is not null)
        {
            TxtNombre.Text = rol.Nombre;
            TxtDescripcion.Text = rol.Descripcion ?? string.Empty;
            ChkActivo.IsChecked = rol.Activo;
            PanelRolBase.Visibility = Visibility.Collapsed;
        }
        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync()
    {
        try
        {
            CmbRolBase.ItemsSource = await _rolesService.ListarAsync(activo: true);
            Rol? detalle = _rol is null
                ? null
                : await _rolesService.ObtenerAsync(_rol.IdRol);
            var permisosAsignados = detalle?.Permisos
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? [];

            var catalogo = await _permisosService.ListarAsync();
            foreach (var permiso in catalogo)
            {
                _permisos.Add(new PermisoFilaDesktop(
                    permiso.Codigo,
                    permiso.Modulo,
                    permiso.Nombre,
                    permisosAsignados.Contains(permiso.Codigo)));
            }
            CmbModulo.ItemsSource = new[] { "Todos" }.Concat(_permisos.Select(p => p.Modulo).Distinct().Order()).ToList();
            ListaPermisos.ItemsSource = _permisos;
            TxtMensajePermisos.Text = $"Selecciona los permisos que desea asignar al rol {Nombre}";
        }
        catch (Exception ex) { MostrarError(ex.Message); }
    }

    private void Info_Click(object sender, RoutedEventArgs e) { PanelInfo.Visibility = Visibility.Visible; PanelPermisos.Visibility = Visibility.Collapsed; }
    private void Permisos_Click(object sender, RoutedEventArgs e) { PanelInfo.Visibility = Visibility.Collapsed; PanelPermisos.Visibility = Visibility.Visible; TxtMensajePermisos.Text = $"Selecciona los permisos que desea asignar al rol {Nombre}"; }
    private void FiltroPermisos_TextChanged(object sender, TextChangedEventArgs e) => FiltrarPermisos();
    private void FiltroPermisos_Changed(object sender, SelectionChangedEventArgs e) => FiltrarPermisos();
    private void FiltrarPermisos()
    {
        var texto = TxtBuscarPermiso.Text.Trim();
        var modulo = CmbModulo.SelectedItem as string;
        ListaPermisos.ItemsSource = _permisos.Where(
            p => (
                string.IsNullOrEmpty(texto) || 
                p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) || 
                p.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase)
            ) && (modulo is null || modulo == "Todos" || p.Modulo == modulo)).ToList();
    }
    private void Guardar_Click(object sender, RoutedEventArgs e) 
    {
        var entrada = new EntradaRol(
            Nombre: TxtNombre.Text.Trim(),
            Descripcion: TxtDescripcion.Text.Trim()
        );

        var resultado = _validador.Validar(entrada);

        if (!HelperValidacion.AplicarResultado(resultado, _controles, MostrarError, () => { }))
            return;

        DialogResult = true; 
    }
    private void MostrarError(string mensaje) { TxtError.Text = mensaje; TxtError.Visibility = Visibility.Visible; }
    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
}

public sealed class PermisoFilaDesktop(string codigo, string modulo, string nombre, bool seleccionado = false)
{
    public string Codigo { get; } = codigo;
    public string Modulo { get; } = modulo;
    public string Nombre { get; } = nombre;
    public bool Seleccionado { get; set; } = seleccionado;
}
