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

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.Windows.Window" />
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
/// <seealso cref="System.Windows.Markup.IStyleConnector" />
public partial class AgregarRolWindow : Window
{
    /// <summary>
    /// 
    /// </summary>
    private sealed class RolBaseOpcion(string nombre, Rol? rol)
    {
        /// <summary>
        /// Gets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        public string Nombre { get; } = nombre;
        /// <summary>
        /// Gets the rol.
        /// </summary>
        /// <value>
        /// The rol.
        /// </value>
        public Rol? Rol { get; } = rol;
    }

    /// <summary>
    /// The validador
    /// </summary>
    private readonly ValidadorRol _validador = new();
    /// <summary>
    /// The controles
    /// </summary>
    private readonly Dictionary<string, Control> _controles;
    /// <summary>
    /// The permisos
    /// </summary>
    private readonly ObservableCollection<PermisoFilaDesktop> _permisos = [];
    /// <summary>
    /// The grupos visibles
    /// </summary>
    private readonly ObservableCollection<PermisoGrupoDesktop> _gruposVisibles = [];
    /// <summary>
    /// The permisos service
    /// </summary>
    private readonly PermisosApiService _permisosService = new();
    /// <summary>
    /// The roles service
    /// </summary>
    private readonly RolesApiService _rolesService = new();
    /// <summary>
    /// Gets the nombre.
    /// </summary>
    /// <value>
    /// The nombre.
    /// </value>
    public string Nombre => TxtNombre.Text.Trim();
    /// <summary>
    /// Gets the descripcion.
    /// </summary>
    /// <value>
    /// The descripcion.
    /// </value>
    public string? Descripcion => 
        string.IsNullOrWhiteSpace(TxtDescripcion.Text) ? null : TxtDescripcion.Text.Trim();
    /// <summary>
    /// Gets the permisos.
    /// </summary>
    /// <value>
    /// The permisos.
    /// </value>
    public IReadOnlyList<string> Permisos => 
        _permisos.Where(p => p.Seleccionado).Select(p => p.Codigo).ToList();

    /// <summary>
    /// Initializes a new instance of the <see cref="AgregarRolWindow"/> class.
    /// </summary>
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

    /// <summary>
    /// Cargars the asynchronous.
    /// </summary>
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

    /// <summary>
    /// Limpiars the error.
    /// </summary>
    private void LimpiarError()
    {
        TxtError.Text = string.Empty;
        TxtError.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Validars the information.
    /// </summary>
    /// <returns></returns>
    private bool ValidarInfo()
    {
        var entrada = new EntradaRol(
            Nombre: TxtNombre.Text.Trim(),
            Descripcion: TxtDescripcion.Text.Trim()
        );

        var resultado = _validador.Validar(entrada);
        return HelperValidacion.AplicarResultado(resultado, _controles, MostrarError, LimpiarError);
    }

    /// <summary>
    /// Handles the Click event of the Siguiente control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Handles the Click event of the Regresar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void Regresar_Click(object sender, RoutedEventArgs e)
    {
        PanelInfo.Visibility = Visibility.Visible;
        PanelPermisos.Visibility = Visibility.Collapsed;
        
        BarraPaso2.Background = new SolidColorBrush(Color.FromRgb(217, 221, 216));
        
        BtnSiguiente.Visibility = Visibility.Visible;
        BtnAgregar.Visibility = Visibility.Collapsed;
        BtnRegresar.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Handles the Click event of the Cancelar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Handles the SelectionChanged event of the RolBase control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
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

    /// <summary>
    /// Copiars the permisos asynchronous.
    /// </summary>
    /// <param name="idRol">The identifier rol.</param>
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

    /// <summary>
    /// Handles the TextChanged event of the FiltroPermisos control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
    private void FiltroPermisos_TextChanged(object sender, TextChangedEventArgs e) => FiltrarPermisos();
    /// <summary>
    /// Handles the Changed event of the FiltroPermisos control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
    private void FiltroPermisos_Changed(object sender, SelectionChangedEventArgs e) => FiltrarPermisos();
    /// <summary>
    /// Filtrars the permisos.
    /// </summary>
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

    /// <summary>
    /// Actualizars the grupos.
    /// </summary>
    private void ActualizarGrupos()
    {
        foreach (var grupo in _gruposVisibles)
            grupo.ActualizarEstado();

        ListaPermisos.Items.Refresh();
    }

    /// <summary>
    /// Handles the Click event of the GrupoSeleccionado control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
