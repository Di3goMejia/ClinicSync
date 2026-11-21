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
    private readonly ObservableCollection<PermisoGrupoDesktop> _gruposVisibles = [];
    private readonly PermisosApiService _permisosService = new();
    private readonly RolesApiService _rolesService = new();
    private bool _rolProtegido;
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

        TxtNombre.TextChanged += (_, _) => LimpiarError();
        TxtDescripcion.TextChanged += (_, _) => LimpiarError();

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
            Rol? detalle = _rol is null
                ? null
                : await _rolesService.ObtenerAsync(_rol.IdRol);
            
            _rolProtegido = detalle?.Protegido == true;
            
            PanelProtegido.Visibility = _rolProtegido ? Visibility.Visible : Visibility.Collapsed;
            ChkActivo.IsEnabled = !_rolProtegido;
            PanelListaPermisos.IsEnabled = !_rolProtegido;

            CmbRolBase.ItemsSource = (await _rolesService.ListarAsync(activo: true))
                .Where(r => _rol is null || r.IdRol != _rol.IdRol)
                .ToList();
            
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
            CmbModulo.SelectedIndex = 0;
            ListaPermisos.ItemsSource = _gruposVisibles;
            
            FiltrarPermisos();
            TxtMensajePermisos.Text = $"Selecciona los permisos que desea asignar al rol {Nombre}";
        }
        catch (Exception ex) { MostrarError(ex.Message); }
    }

    private void Info_Click(object sender, RoutedEventArgs e) 
    { 
        PanelInfo.Visibility = Visibility.Visible; 
        PanelPermisos.Visibility = Visibility.Collapsed; 
    }
    
    private void Permisos_Click(object sender, RoutedEventArgs e) 
    { 
        PanelInfo.Visibility = Visibility.Collapsed; 
        PanelPermisos.Visibility = Visibility.Visible; 
        TxtMensajePermisos.Text = $"Selecciona los permisos que desea asignar al rol {Nombre}"; 
    }
    
    private void FiltroPermisos_TextChanged(object sender, TextChangedEventArgs e) => FiltrarPermisos();
    private void FiltroPermisos_Changed(object sender, SelectionChangedEventArgs e) => FiltrarPermisos();
    
    private void FiltrarPermisos()
    {
        var texto = TxtBuscarPermiso.Text.Trim();
        var modulo = CmbModulo.SelectedItem as string;
        var permisosFiltrados = _permisos.Where(p =>
            (string.IsNullOrEmpty(texto) ||
             p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
             p.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase)) &&
            (modulo is null || modulo == "Todos" || p.Modulo == modulo));

        _gruposVisibles.Clear();
        foreach (var grupo in permisosFiltrados.GroupBy(p => p.Modulo).OrderBy(g => g.Key))
            _gruposVisibles.Add(new PermisoGrupoDesktop(grupo.Key, grupo));
    }

    private void GrupoSeleccionado_Click(object sender, RoutedEventArgs e)
    {
        if (_rolProtegido || sender is not CheckBox checkBox || checkBox.DataContext is not PermisoGrupoDesktop grupo)
            return;

        var seleccionar = checkBox.IsChecked == true;
        foreach (var permiso in grupo.Permisos)
            permiso.Seleccionado = seleccionar;

        grupo.ActualizarEstado();
        ListaPermisos.Items.Refresh();
        e.Handled = true;
    }

    private void PermisoSeleccionado_Click(object sender, RoutedEventArgs e)
    {
        if (_rolProtegido)
            return;

        if (sender is CheckBox { DataContext: PermisoFilaDesktop permiso })
        {
            foreach (var grupo in _gruposVisibles.Where(g => g.Permisos.Contains(permiso)))
                grupo.ActualizarEstado();

            ListaPermisos.Items.Refresh();
        }
    }
    private void LimpiarError()
    {
        TxtError.Text = string.Empty;
        TxtError.Visibility = Visibility.Collapsed;
    }

    private void Guardar_Click(object sender, RoutedEventArgs e) 
    {
        LimpiarError();
        var entrada = new EntradaRol(
            Nombre: TxtNombre.Text.Trim(),
            Descripcion: TxtDescripcion.Text.Trim()
        );

        var resultado = _validador.Validar(entrada);

        if (!HelperValidacion.AplicarResultado(resultado, _controles, MostrarError, LimpiarError))
            return;

        DialogResult = true; 
    }
    private void MostrarError(string mensaje) { TxtError.Text = mensaje; TxtError.Visibility = Visibility.Visible; }
    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        bool cambios = _rol is null ||
            TxtNombre.Text.Trim() != _rol.Nombre ||
            (TxtDescripcion.Text.Trim() != (_rol.Descripcion ?? string.Empty)) ||
            (ChkActivo.IsChecked == true) != _rol.Activo;

        if (cambios)
        {
            var descarte = new ConfirmarDescarteWindow("¿Seguro que desea descartar esta edición?") { Owner = this };
            if (descarte.ShowDialog() != true)
                return;
        }

        DialogResult = false;
    }
    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
        if (e.LeftButton == MouseButtonState.Pressed) 
            DragMove(); 
    }
}

public sealed class PermisoFilaDesktop(
    string codigo, string modulo, string nombre, bool seleccionado = false)
{
    public string Codigo { get; } = codigo;
    public string Modulo { get; } = modulo;
    public string Nombre { get; } = nombre;
    public bool Seleccionado { get; set; } = seleccionado;
}

public sealed class PermisoGrupoDesktop
{
    public PermisoGrupoDesktop(string nombre, IEnumerable<PermisoFilaDesktop> permisos)
    {
        Nombre = nombre;
        Permisos = [.. permisos];
        ActualizarEstado();
    }

    public string Nombre { get; }
    public IReadOnlyList<PermisoFilaDesktop> Permisos { get; }
    public bool EstaExpandido { get; set; } = true;
    public bool? TodosSeleccionados { get; private set; }

    public void ActualizarEstado()
    {
        TodosSeleccionados = Permisos.Count > 0 && Permisos.All(p => p.Seleccionado)
            ? true
            : Permisos.Any(p => p.Seleccionado) ? null : false;
    }
}
