using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ClinicSyncDesktop.Views
{
    // MATERIAL QUE SE PUEDE ELEGIR EN UN EGRESO
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class MaterialParaEgreso : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the identifier material.
        /// </summary>
        /// <value>
        /// The identifier material.
        /// </value>
        public int IdMaterial { get; set; }
        /// <summary>
        /// Gets or sets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        public string Nombre { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the unidad medida.
        /// </summary>
        /// <value>
        /// The unidad medida.
        /// </value>
        public string UnidadMedida { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the stock actual.
        /// </summary>
        /// <value>
        /// The stock actual.
        /// </value>
        public decimal? StockActual { get; set; }
        /// <summary>
        /// Gets or sets the limite disponible.
        /// </summary>
        /// <value>
        /// The limite disponible.
        /// </value>
        public decimal? LimiteDisponible { get; set; }

        /// <summary>
        /// The seleccionado
        /// </summary>
        private bool _seleccionado;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MaterialParaEgreso"/> is seleccionado.
        /// </summary>
        /// <value>
        ///   <c>true</c> if seleccionado; otherwise, <c>false</c>.
        /// </value>
        public bool Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (_seleccionado == value) return;
                _seleccionado = value;
                Notificar();
            }
        }

        /// <summary>
        /// The cantidad
        /// </summary>
        private decimal _cantidad = 1;
        /// <summary>
        /// Gets or sets the cantidad.
        /// </summary>
        /// <value>
        /// The cantidad.
        /// </value>
        public decimal Cantidad
        {
            get => _cantidad;
            set
            {
                if (_cantidad == value) return;
                _cantidad = value;
                Notificar();
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notificars the specified nombre.
        /// </summary>
        /// <param name="nombre">The nombre.</param>
        private void Notificar([CallerMemberName] string? nombre = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class SeleccionarMaterialesWindow : Window
    {
        /// <summary>
        /// The previos
        /// </summary>
        private readonly List<MaterialParaEgreso> _previos;
        /// <summary>
        /// The todos
        /// </summary>
        private readonly ObservableCollection<MaterialParaEgreso> _todos = new();
        /// <summary>
        /// The vista
        /// </summary>
        private ICollectionView? _vista;

        /// <summary>
        /// Gets the seleccionados.
        /// </summary>
        /// <value>
        /// The seleccionados.
        /// </value>
        public List<MaterialParaEgreso> Seleccionados { get; private set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SeleccionarMaterialesWindow"/> class.
        /// </summary>
        /// <param name="previos">The previos.</param>
        public SeleccionarMaterialesWindow(
            IEnumerable<MaterialParaEgreso>? previos = null)
        {
            InitializeComponent();

            _previos = previos?.ToList() ?? new List<MaterialParaEgreso>();
            Loaded += Ventana_Loaded;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeleccionarMaterialesWindow"/> class.
        /// </summary>
        /// <param name="catalogo">The catalogo.</param>
        /// <param name="seleccionados">The seleccionados.</param>
        /// <param name="stockInventario">The stock inventario.</param>
        public SeleccionarMaterialesWindow(
            IEnumerable<MaterialIngreso> catalogo,
            IEnumerable<MaterialConsumido> seleccionados,
            IReadOnlyDictionary<int, decimal>? stockInventario = null)
        {
            InitializeComponent();

            _previos = new List<MaterialParaEgreso>();
            var cantidades = seleccionados.ToDictionary(m => m.IdMaterial, m => m.Cantidad);
            foreach (var material in catalogo.Where(m => m.Activo).OrderBy(m => m.Nombre))
            {
                bool elegido = cantidades.TryGetValue(material.IdMaterial, out decimal cantidad);
                _todos.Add(new MaterialParaEgreso
                {
                    IdMaterial = material.IdMaterial,
                    Nombre = material.Nombre,
                    StockActual = stockInventario is null
                        ? material.StockActual
                        : stockInventario.TryGetValue(material.IdMaterial, out decimal stock) ? stock : null,
                    LimiteDisponible = material.StockActual,
                    Seleccionado = elegido && cantidad > 0,
                    Cantidad = elegido ? cantidad : Math.Min(1m, material.StockActual)
                });
            }

            MostrarCatalogo();
        }

        /// <summary>
        /// Elegirs the para ingreso.
        /// </summary>
        /// <param name="propietario">The propietario.</param>
        /// <param name="catalogo">The catalogo.</param>
        /// <param name="seleccionados">The seleccionados.</param>
        /// <param name="stockInventario">The stock inventario.</param>
        /// <returns></returns>
        public static List<MaterialConsumido>? ElegirParaIngreso(
            Window propietario,
            IEnumerable<MaterialIngreso> catalogo,
            IEnumerable<MaterialConsumido> seleccionados,
            IReadOnlyDictionary<int, decimal>? stockInventario = null)
        {
            var selector = new SeleccionarMaterialesWindow(catalogo, seleccionados, stockInventario)
            {
                Owner = propietario
            };

            return selector.ShowDialog() == true
                ? selector.Seleccionados.Select(m => new MaterialConsumido
                {
                    IdMaterial = m.IdMaterial,
                    Nombre = m.Nombre,
                    Cantidad = m.Cantidad
                }).ToList()
                : null;
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the Encabezado control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Encabezado_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // CARGAR SOLO MATERIALES ACTIVOS
        /// <summary>
        /// Handles the Loaded event of the Ventana control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Ventana_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                var lista =
                    await ClienteApi.ObtenerAutenticadoAsync<List<MaterialParaEgreso>>(
                        "api/inventario/materiales/?Activo=true");

                foreach (var material in lista)
                {
                    var previo = _previos.FirstOrDefault(
                        p => p.IdMaterial == material.IdMaterial);

                    if (previo is not null)
                    {
                        material.Cantidad = previo.Cantidad;
                        material.Seleccionado = true;
                    }

                    _todos.Add(material);
                }

                // Mantener visibles los materiales históricos para que se puedan quitar.
                foreach (var previo in _previos.Where(p => _todos.All(m => m.IdMaterial != p.IdMaterial)))
                {
                    _todos.Add(new MaterialParaEgreso
                    {
                        IdMaterial = previo.IdMaterial,
                        Nombre = previo.Nombre,
                        UnidadMedida = previo.UnidadMedida,
                        Cantidad = previo.Cantidad,
                        Seleccionado = true
                    });
                }

                MostrarCatalogo();

                if (_todos.Count == 0)
                {
                    txtError.Text = "No hay materiales activos en el inventario.";
                }
            }
            catch (Exception)
            {
                txtError.Text =
                    "No fue posible cargar los materiales. Verifica el servidor y tus permisos.";
            }
        }

        /// <summary>
        /// Mostrars the catalogo.
        /// </summary>
        private void MostrarCatalogo()
        {
            _vista = CollectionViewSource.GetDefaultView(_todos);
            _vista.Filter = Filtrar;
            listaMateriales.ItemsSource = _vista;
        }

        /// <summary>
        /// Filtrars the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool Filtrar(object obj)
        {
            if (obj is not MaterialParaEgreso material)
                return false;

            string texto = txtBuscar.Text.Trim();

            return texto.Length == 0 ||
                   material.Nombre.Contains(
                       texto,
                       StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handles the TextChanged event of the txtBuscar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void txtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            _vista?.Refresh();
        }

        /// <summary>
        /// Handles the Click event of the Menos control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Menos_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is MaterialParaEgreso m &&
                m.Cantidad > 1)
            {
                m.Cantidad = Math.Max(1m, m.Cantidad - 1m);
            }
        }

        /// <summary>
        /// Handles the Click event of the Mas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Mas_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is MaterialParaEgreso m)
            {
                decimal maximo = m.LimiteDisponible ?? 99999m;
                if (m.Cantidad < maximo)
                    m.Cantidad = Math.Min(maximo, m.Cantidad + 1m);
            }
        }

        /// <summary>
        /// Handles the Click event of the Cerrar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the Click event of the Ok control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var seleccion = _todos.Where(m => m.Seleccionado).ToList();
            var invalido = seleccion.FirstOrDefault(m =>
                m.LimiteDisponible is decimal disponible &&
                (m.Cantidad <= 0 || m.Cantidad > disponible));
            if (invalido != null)
            {
                txtError.Text = $"Stock insuficiente de {invalido.Nombre}. Disponible: {invalido.LimiteDisponible:N2}.";
                return;
            }

            Seleccionados = seleccion;
            DialogResult = true;
        }
    }
}
