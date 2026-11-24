using ClinicSyncDesktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ClinicSyncDesktop.Views
{

    /// <summary>
    /// 
    /// </summary>
    public class TratamientoFila
    {
        /// <summary>
        /// Gets or sets the tratamiento.
        /// </summary>
        /// <value>
        /// The tratamiento.
        /// </value>
        public TratamientoIngreso Tratamiento { get; set; } = new();
        /// <summary>
        /// Gets the identifier tratamiento visita.
        /// </summary>
        /// <value>
        /// The identifier tratamiento visita.
        /// </value>
        public int IdTratamientoVisita =>
            Tratamiento.IdTratamientoVisita;
        /// <summary>
        /// Gets the nombre.
        /// </summary>
        /// <value>
        /// The nombre.
        /// </value>
        public string Nombre => Tratamiento.Nombre;
        /// <summary>
        /// Gets the precio.
        /// </summary>
        /// <value>
        /// The precio.
        /// </value>
        public decimal Precio => Tratamiento.Precio;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TratamientoFila"/> is seleccionado.
        /// </summary>
        /// <value>
        ///   <c>true</c> if seleccionado; otherwise, <c>false</c>.
        /// </value>
        public bool Seleccionado { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class MaterialFila
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
        public string Nombre { get; set; } = "";
        /// <summary>
        /// Gets or sets the stock actual.
        /// </summary>
        /// <value>
        /// The stock actual.
        /// </value>
        public decimal StockActual { get; set; }
        /// <summary>
        /// Gets or sets the cantidad texto.
        /// </summary>
        /// <value>
        /// The cantidad texto.
        /// </value>
        public string CantidadTexto { get; set; } = "0";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class SeleccionIngresoWindow : Window
    {
        /// <summary>
        /// 
        /// </summary>
        private enum TipoSeleccion
        {
            /// <summary>
            /// The pacientes
            /// </summary>
            Pacientes,
            /// <summary>
            /// The tratamientos
            /// </summary>
            Tratamientos,
            /// <summary>
            /// The materiales
            /// </summary>
            Materiales
        }

        /// <summary>
        /// The tipo
        /// </summary>
        private readonly TipoSeleccion tipo;
        /// <summary>
        /// The pacientes
        /// </summary>
        private readonly List<PacienteIngreso> pacientes = new();
        /// <summary>
        /// The tratamientos
        /// </summary>
        private readonly List<TratamientoFila> tratamientos = new();
        /// <summary>
        /// The materiales
        /// </summary>
        private readonly ObservableCollection<MaterialFila> materiales = new();

        /// <summary>
        /// Gets the paciente elegido.
        /// </summary>
        /// <value>
        /// The paciente elegido.
        /// </value>
        public PacienteIngreso? PacienteElegido { get; private set; }

        /// <summary>
        /// Gets the tratamientos elegidos.
        /// </summary>
        /// <value>
        /// The tratamientos elegidos.
        /// </value>
        public List<TratamientoIngreso> TratamientosElegidos
        { get; private set; } = new();

        /// <summary>
        /// Gets the materiales elegidos.
        /// </summary>
        /// <value>
        /// The materiales elegidos.
        /// </value>
        public List<MaterialConsumido> MaterialesElegidos
        { get; private set; } = new();

        // CONSTRUCTOR PARA PACIENTES
        /// <summary>
        /// Initializes a new instance of the <see cref="SeleccionIngresoWindow"/> class.
        /// </summary>
        /// <param name="catalogo">The catalogo.</param>
        /// <param name="seleccionado">The seleccionado.</param>
        public SeleccionIngresoWindow(
            IEnumerable<PacienteIngreso> catalogo,
            PacienteIngreso? seleccionado)
        {
            InitializeComponent();

            tipo = TipoSeleccion.Pacientes;
            TxtTitulo.Text = "Seleccionar paciente";

            pacientes = catalogo
                .Where(p => p.Activo ||
                    p.IdPaciente == seleccionado?.IdPaciente)
                .OrderBy(p => p.NombreCompleto)
                .ToList();

            ContenedorPacientes.Visibility = Visibility.Visible;
            LstPacientes.ItemsSource = pacientes;

            LstPacientes.SelectedItem = pacientes.FirstOrDefault(
                p => p.IdPaciente == seleccionado?.IdPaciente);
        }

        // CONSTRUCTOR PARA TRATAMIENTOS
        /// <summary>
        /// Initializes a new instance of the <see cref="SeleccionIngresoWindow"/> class.
        /// </summary>
        /// <param name="catalogo">The catalogo.</param>
        /// <param name="seleccionados">The seleccionados.</param>
        public SeleccionIngresoWindow(
            IEnumerable<TratamientoIngreso> catalogo,
            IEnumerable<TratamientoIngreso> seleccionados)
        {
            InitializeComponent();

            tipo = TipoSeleccion.Tratamientos;
            TxtTitulo.Text = "Seleccionar tratamientos";
            var ids = seleccionados
                .Select(ClaveTratamiento)
                .ToHashSet();

            tratamientos = catalogo
                .GroupBy(ClaveTratamiento)
                .Select(g => g.First())
                .Select(t => new TratamientoFila
                {
                    Tratamiento = t,
                    Seleccionado =
                        ids.Contains(ClaveTratamiento(t))
                })
                .ToList();

            ContenedorTratamientos.Visibility = Visibility.Visible;
            LstTratamientos.ItemsSource = tratamientos;

        }

        // CONSTRUCTOR PARA MATERIALES
        /// <summary>
        /// Initializes a new instance of the <see cref="SeleccionIngresoWindow"/> class.
        /// </summary>
        /// <param name="catalogo">The catalogo.</param>
        /// <param name="seleccionados">The seleccionados.</param>
        public SeleccionIngresoWindow(
            IEnumerable<MaterialIngreso> catalogo,
            IEnumerable<MaterialConsumido> seleccionados)
        {
            InitializeComponent();

            tipo = TipoSeleccion.Materiales;
            TxtTitulo.Text = "Seleccionar materiales";

            var cantidades = seleccionados
                .ToDictionary(m => m.IdMaterial, m => m.Cantidad);

            materiales = new ObservableCollection<MaterialFila>(
                catalogo
                    .Where(m => m.Activo)
                    .Select(m => new MaterialFila
                    {
                        IdMaterial = m.IdMaterial,
                        Nombre = m.Nombre,
                        StockActual = m.StockActual,
                        CantidadTexto = cantidades.TryGetValue(
                            m.IdMaterial, out var cantidad)
                            ? cantidad.ToString(CultureInfo.CurrentCulture)
                            : "0"
                    }));

            ContenedorMateriales.Visibility = Visibility.Visible;
            DgMateriales.ItemsSource = materiales;
        }

        /// <summary>
        /// Claves the tratamiento.
        /// </summary>
        /// <param name="tratamiento">The tratamiento.</param>
        /// <returns></returns>
        private static (bool EsDeVisita, int Id) ClaveTratamiento(TratamientoIngreso tratamiento)
        {
            return tratamiento.IdTratamientoVisita > 0
                ? (true, tratamiento.IdTratamientoVisita)
                : (false, tratamiento.IdTratamiento);
        }

        private void SeleccionarPaciente_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: PacienteIngreso paciente })
            {
                LstPacientes.SelectedItem = paciente;
                TxtError.Text = "";
            }
        }

        // BUSCADOR DE LOS TRES MODOS
        private void Buscar_TextChanged(
            object sender, TextChangedEventArgs e)
        {
            if (TxtBuscar == null)
                return;

            string texto = TxtBuscar.Text.Trim();

            if (tipo == TipoSeleccion.Pacientes)
            {
                var vista = CollectionViewSource.GetDefaultView(
                    LstPacientes.ItemsSource);

                if (vista == null) return;

                var pacienteAnterior = LstPacientes.SelectedItem;
                vista.Filter = objeto =>
                    objeto is PacienteIngreso p &&
                    p.NombreCompleto.Contains(
                        texto, StringComparison.OrdinalIgnoreCase);

                if (pacienteAnterior != null && vista.Contains(pacienteAnterior))
                    LstPacientes.SelectedItem = pacienteAnterior;
            }
            else if (tipo == TipoSeleccion.Tratamientos)
            {
                var vista = CollectionViewSource.GetDefaultView(
                    LstTratamientos.ItemsSource);

                if (vista == null) return;

                vista.Filter = objeto =>
                    objeto is TratamientoFila t &&
                    t.Nombre.Contains(
                        texto, StringComparison.OrdinalIgnoreCase);

            }
            else
            {
                var vista = CollectionViewSource.GetDefaultView(
                    DgMateriales.ItemsSource);

                if (vista == null) return;

                vista.Filter = objeto =>
                    objeto is MaterialFila m &&
                    m.Nombre.Contains(
                        texto, StringComparison.OrdinalIgnoreCase);

            }
        }

        // CONFIRMAR SELECCIÓN
        private void Aceptar_Click(
            object sender, RoutedEventArgs e)
        {
            TxtError.Text = "";

            if (tipo == TipoSeleccion.Pacientes)
            {
                if (LstPacientes.SelectedItem is not PacienteIngreso paciente)
                {
                    TxtError.Text = "Seleccione un paciente.";
                    return;
                }

                PacienteElegido = paciente;
                DialogResult = true;
                return;
            }

            if (tipo == TipoSeleccion.Tratamientos)
            {
                TratamientosElegidos = tratamientos
                    .Where(t => t.Seleccionado)
                    .Select(t => t.Tratamiento)
                    .ToList();

                if (TratamientosElegidos.Count == 0)
                {
                    TxtError.Text =
                        "Seleccione al menos un tratamiento.";
                    return;
                }

                DialogResult = true;
                return;
            }

            // VALIDAR CANTIDADES DE MATERIALES
            var seleccion = new List<MaterialConsumido>();

            foreach (var m in materiales)
            {
                if (!decimal.TryParse(
                    m.CantidadTexto,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out decimal cantidad))
                {
                    TxtError.Text =
                        $"La cantidad de {m.Nombre} no es válida.";
                    return;
                }

                if (cantidad < 0)
                {
                    TxtError.Text =
                        "Las cantidades no pueden ser negativas.";
                    return;
                }

                if (cantidad > m.StockActual)
                {
                    TxtError.Text =
                        $"Stock insuficiente de {m.Nombre}. " +
                        $"Disponible: {m.StockActual:N2}.";
                    return;
                }

                if (cantidad > 0)
                {
                    seleccion.Add(new MaterialConsumido
                    {
                        IdMaterial = m.IdMaterial,
                        Nombre = m.Nombre,
                        Cantidad = cantidad
                    });
                }
            }

            MaterialesElegidos = seleccion;
            DialogResult = true;
        }

        private void Cancelar_Click(
            object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}