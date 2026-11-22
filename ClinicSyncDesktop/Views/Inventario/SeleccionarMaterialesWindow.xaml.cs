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
    public class MaterialParaEgreso : INotifyPropertyChanged
    {
        public int IdMaterial { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal? StockActual { get; set; }
        public decimal? LimiteDisponible { get; set; }

        private bool _seleccionado;
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

        private decimal _cantidad = 1;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notificar([CallerMemberName] string? nombre = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }

    public partial class SeleccionarMaterialesWindow : Window
    {
        private readonly List<MaterialParaEgreso> _previos;
        private readonly ObservableCollection<MaterialParaEgreso> _todos = new();
        private ICollectionView? _vista;

        public List<MaterialParaEgreso> Seleccionados { get; private set; } = new();

        public SeleccionarMaterialesWindow(
            IEnumerable<MaterialParaEgreso>? previos = null)
        {
            InitializeComponent();

            _previos = previos?.ToList() ?? new List<MaterialParaEgreso>();
            Loaded += Ventana_Loaded;
        }

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

        private void Encabezado_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // CARGAR SOLO MATERIALES ACTIVOS
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

        private void MostrarCatalogo()
        {
            _vista = CollectionViewSource.GetDefaultView(_todos);
            _vista.Filter = Filtrar;
            listaMateriales.ItemsSource = _vista;
        }

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

        private void txtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            _vista?.Refresh();
        }

        private void Menos_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is MaterialParaEgreso m &&
                m.Cantidad > 1)
            {
                m.Cantidad = Math.Max(1m, m.Cantidad - 1m);
            }
        }

        private void Mas_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is MaterialParaEgreso m)
            {
                decimal maximo = m.LimiteDisponible ?? 99999m;
                if (m.Cantidad < maximo)
                    m.Cantidad = Math.Min(maximo, m.Cantidad + 1m);
            }
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

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
