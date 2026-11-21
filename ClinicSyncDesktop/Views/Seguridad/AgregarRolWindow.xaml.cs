using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using ClinicSyncDesktop.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views.Seguridad;

public partial class AgregarRolWindow : Window
{
    private sealed class RolBaseOpcion(string nombre, Rol? rol)
    {
        public string Nombre { get; } = nombre;
        public Rol? Rol { get; } = rol;
    }

    private readonly ValidadorRol _validador = new();
    private readonly Dictionary<string, Control> _controles;
    private readonly ObservableCollection<PermisoFilaDesktop> _permisos = [];
    private readonly ObservableCollection<PermisoGrupoDesktop> _gruposVisibles = [];
    private readonly PermisosApiService _permisosService = new();
    private readonly RolesApiService _rolesService = new();
    public string Nombre => TxtNombre.Text.Trim();
    public string? Descripcion => 
        string.IsNullOrWhiteSpace(TxtDescripcion.Text) ? null : TxtDescripcion.Text.Trim();
    public IReadOnlyList<string> Permisos => 
        _permisos.Where(p => p.Seleccionado).Select(p => p.Codigo).ToList();

    public AgregarRolWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await CargarAsync();

        _controles = new Dictionary<string, Control>
        {
            [nameof(EntradaRol.Nombre)] = TxtNombre,
            [nameof(EntradaRol.Descripcion)] = TxtDescripcion
        };

        TxtNombre.TextChanged += (_, _) => LimpiarError();
        TxtDescripcion.TextChanged += (_, _) => LimpiarError();
    }

    private async Task CargarAsync()
    {
        try
        {
            var modulos = new List<string> { "Todos" };

            var roles = await _rolesService.ListarAsync(activo: true);

            CmbRolBase.ItemsSource = roles
                .Select(rol => new RolBaseOpcion(rol.Nombre, rol))
                .Prepend(new RolBaseOpcion("Sin rol base", null))
                .ToList();

            CmbRolBase.SelectedIndex = 0;

            foreach (var permiso in await _permisosService.ListarAsync())
                _permisos.Add(new PermisoFilaDesktop(permiso.Codigo, permiso.Modulo, permiso.Nombre));
            
            CmbModulo.ItemsSource = modulos.Concat(_permisos.Select(p => p.Modulo).Distinct().Order()).ToList();
            CmbModulo.SelectedIndex = 0;
            ListaPermisos.ItemsSource = _gruposVisibles;
            FiltrarPermisos();
        }
        catch (Exception ex) 
        { 
            MostrarError(ex.Message); 
        }
    }

    private void LimpiarError()
    {
        TxtError.Text = string.Empty;
        TxtError.Visibility = Visibility.Collapsed;
    }

    private bool ValidarInfo()
    {
        var entrada = new EntradaRol(
            Nombre: TxtNombre.Text.Trim(),
            Descripcion: TxtDescripcion.Text.Trim()
        );

        var resultado = _validador.Validar(entrada);
        return HelperValidacion.AplicarResultado(resultado, _controles, MostrarError, LimpiarError);
    }

    private void Siguiente_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarInfo())
            return;

        PanelInfo.Visibility = Visibility.Collapsed;
        PanelPermisos.Visibility = Visibility.Visible;

        BarraPaso1.Background = (Brush)FindResource("Turquesa");
        BarraPaso2.Background = (Brush)FindResource("Turquesa");
        
        BtnSiguiente.Visibility = Visibility.Collapsed;
        BtnAgregar.Visibility = Visibility.Visible;
        BtnRegresar.Visibility = Visibility.Visible;

        TxtMensajePermisos.Text = $"Selecciona los permisos que desea asignar al rol {Nombre}";
    }

    private void Regresar_Click(object sender, RoutedEventArgs e)
    {
        PanelInfo.Visibility = Visibility.Visible;
        PanelPermisos.Visibility = Visibility.Collapsed;
        
        BarraPaso2.Background = new SolidColorBrush(Color.FromRgb(217, 221, 216));
        
        BtnSiguiente.Visibility = Visibility.Visible;
        BtnAgregar.Visibility = Visibility.Collapsed;
        BtnRegresar.Visibility = Visibility.Collapsed;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        bool hayDatos = !string.IsNullOrWhiteSpace(TxtNombre.Text) ||
            !string.IsNullOrWhiteSpace(TxtDescripcion.Text) ||
            _permisos.Any(p => p.Seleccionado);

        if (hayDatos)
        {
            var descarte = new ConfirmarDescarteWindow("¿Seguro que desea descartar este registro?") { Owner = this };
            if (descarte.ShowDialog() != true)
                return;
        }

        DialogResult = false;
    }

    private void RolBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbRolBase.SelectedItem is not RolBaseOpcion opcion || _permisos.Count == 0)
            return;

        foreach (var permiso in _permisos)
            permiso.Seleccionado = false;

        if (opcion.Rol is null)
        {
            ActualizarGrupos();
            return;
        }

        _ = CopiarPermisosAsync(opcion.Rol.IdRol);
    }

    private async Task CopiarPermisosAsync(int idRol)
    {
        try
        {
            var detalle = await _rolesService.ObtenerAsync(idRol);
            var seleccionados = detalle.Permisos.ToHashSet(StringComparer.OrdinalIgnoreCase);
            
            foreach (var permiso in _permisos)
                permiso.Seleccionado = seleccionados.Contains(permiso.Codigo);

            ActualizarGrupos();
        }
        catch (Exception ex) 
        { 
            MostrarError(ex.Message); 
        }
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

    private void ActualizarGrupos()
    {
        foreach (var grupo in _gruposVisibles)
            grupo.ActualizarEstado();

        ListaPermisos.Items.Refresh();
    }

    private void GrupoSeleccionado_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox || checkBox.DataContext is not PermisoGrupoDesktop grupo)
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
        if (sender is CheckBox { DataContext: PermisoFilaDesktop permiso })
        {
            foreach (var grupo in _gruposVisibles.Where(g => g.Permisos.Contains(permiso)))
                grupo.ActualizarEstado();

            ListaPermisos.Items.Refresh();
        }
    }
    
    private void Guardar_Click(object sender, RoutedEventArgs e) 
    {
        if (!ValidarInfo())
            return;

        DialogResult = true; 
    }

public sealed class PermisoGrupoDesktop
{
    public PermisoGrupoDesktop(string nombre, IEnumerable<PermisoFilaDesktop> permisos)
    {
        Nombre = nombre;
        Permisos = permisos.ToList();
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

    private void MostrarError(string mensaje) 
    { 
        TxtError.Text = mensaje; 
        TxtError.Visibility = Visibility.Visible; 
    }

    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
    { 
        if (e.LeftButton == MouseButtonState.Pressed) 
            DragMove(); 
    }
}
