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

    public class TratamientoFila
    {
        public TratamientoIngreso Tratamiento { get; set; } = new();
        public int IdTratamientoVisita =>
            Tratamiento.IdTratamientoVisita;
        public string Nombre => Tratamiento.Nombre;
        public decimal Precio => Tratamiento.Precio;
        public bool Seleccionado { get; set; }
    }


    public class MaterialFila
    {
        public int IdMaterial { get; set; }
        public string Nombre { get; set; } = "";
        public decimal StockActual { get; set; }
        public string CantidadTexto { get; set; } = "0";
    }

    public partial class SeleccionIngresoWindow : Window
    {
        private enum TipoSeleccion
        {
            Pacientes,
            Tratamientos,
            Materiales
        }

        private readonly TipoSeleccion tipo;
        private readonly List<PacienteIngreso> pacientes = new();
        private readonly List<TratamientoFila> tratamientos = new();
        private readonly ObservableCollection<MaterialFila> materiales = new();

        public PacienteIngreso? PacienteElegido { get; private set; }

        public List<TratamientoIngreso> TratamientosElegidos
        { get; private set; } = new();

        public List<MaterialConsumido> MaterialesElegidos
        { get; private set; } = new();

        // CONSTRUCTOR PARA PACIENTES
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