using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ClinicSyncDesktop.Data;

namespace ClinicSyncDesktop.Views
{

    public class PacienteVista : INotifyPropertyChanged
    {
        public int IdPaciente { get; set; }

        public string Identidad { get; set; } = "";
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public string Sexo { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = "";

        private bool activo = true;

        public bool Activo
        {
            get => activo;
            set
            {
                if (activo == value)
                    return;

                activo = value;

                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(Activo)));
            }
        }

        public string NombreCompleto => Nombres + " " + Apellidos;

        public int Edad
        {
            get
            {
                DateTime hoy = DateTime.Today;
                int edad = hoy.Year - FechaNacimiento.Year;

                if (FechaNacimiento.Date > hoy.AddYears(-edad))
                    edad--;

                return edad;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public sealed class PacienteApi
    {
        public int IdPaciente { get; set; }
        public string Identidad { get; set; } = "";
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public string Genero { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = "";
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
    }

    public partial class PacientesView : UserControl
    {
        private readonly ObservableCollection<PacienteVista> pacientes = new();
        private readonly ICollectionView vistaPacientes;

        private bool cargandoPacientes = false;
        private bool cambiandoEstado = false;

        public PacientesView()
        {
            InitializeComponent();

            vistaPacientes =
                CollectionViewSource.GetDefaultView(pacientes);

            vistaPacientes.Filter = FiltrarPaciente;

            dgPacientes.ItemsSource = vistaPacientes;

            Loaded += PacientesView_Loaded;
        }

        // MOSTRAR NOTIFICACIÓN PERSONALIZADA
        private void MostrarAviso(
            string mensaje,
            bool exitoso)
        {
            PanelAvisos.Children.Clear();

            AvisoPaciente aviso =
                new AvisoPaciente(mensaje, exitoso);

            PanelAvisos.Children.Add(aviso);
        }

        // CARGAR PACIENTES
        private async void PacientesView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await CargarPacientesAsync();
            }
            catch (Exception ex)
            {
                MostrarAviso(
                    "Error al cargar pacientes.",
                    false);

                MessageBox.Show(
                    ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // CONSULTAR SQL SERVER
        private async Task CargarPacientesAsync()
        {
            if (cargandoPacientes)
                return;

            cargandoPacientes = true;

            try
            {
                PacienteApi[] lista =
                    await ClienteApi
                        .ObtenerAutenticadoAsync<PacienteApi[]>(
                            "api/pacientes");

                pacientes.Clear();

                foreach (PacienteApi p in lista)
                {
                    pacientes.Add(new PacienteVista
                    {
                        IdPaciente = p.IdPaciente,
                        Identidad = p.Identidad,
                        Nombres = p.Nombres,
                        Apellidos = p.Apellidos,
                        Sexo = p.Genero,
                        FechaNacimiento = p.FechaNacimiento,
                        Telefono = p.Telefono,
                        Activo = p.Activo
                    });
                }

                vistaPacientes.Refresh();
            }
            finally
            {
                cargandoPacientes = false;
            }
        }

        // BUSCAR PACIENTES

        private bool FiltrarPaciente(object objeto)
        {
            if (objeto is not PacienteVista paciente)
                return false;

            string buscar = txtBuscar.Text.Trim();

            // Si la búsqueda está vacía, mostrar todos.
            if (string.IsNullOrWhiteSpace(buscar))
                return true;

            // Buscar por nombre completo, sin distinguir
            // entre mayúsculas y minúsculas.
            return paciente.NombreCompleto.Contains(
                buscar,
                StringComparison.OrdinalIgnoreCase);
        }

        private void txtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            // Actualizar los resultados mientras escribimos.
            vistaPacientes?.Refresh();
        }


        // AGREGAR PACIENTE
        private async void btnAgregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            AgregarPacienteWindow ventana =
                new AgregarPacienteWindow();

            ventana.IdentidadYaRegistrada = identidad =>
                pacientes.Any(p =>
                    new string(
                        p.Identidad.Where(char.IsDigit).ToArray()
                    ) == identidad);

            ventana.Owner = Window.GetWindow(this);

            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                try
                {
                    await CargarPacientesAsync();

                    MostrarAviso(
                        "Paciente registrado exitosamente.",
                        true);
                }
                catch (Exception ex)
                {
                    MostrarAviso(
                        "Paciente guardado, pero no se pudo actualizar la tabla.",
                        false);

                    MessageBox.Show(
                        ex.Message,
                        "ClinicSync",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            else if (ventana.ErrorAlRegistrar)
            {
                MostrarAviso(
                    "Error al agregar registro.",
                    false);
            }
        }

        // CAMBIAR ESTADO
        private async void Estado_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (cambiandoEstado)
                return;

            if (sender is not CheckBox check ||
                check.DataContext is not PacienteVista paciente)
                return;

            if (paciente.IdPaciente <= 0)
            {
                MostrarAviso(
                    "No se encontró el paciente.",
                    false);
                return;
            }

            bool estadoAnterior = !paciente.Activo;

            cambiandoEstado = true;
            check.IsEnabled = false;

            try
            {
                await ClienteApi.CambiarEstadoPacienteAsync(
                    paciente.IdPaciente,
                    paciente.Activo);
            }
            catch (Exception)
            {
                paciente.Activo = estadoAnterior;

                MostrarAviso(
                    "Error al actualizar el estado.",
                    false);
            }
            finally
            {
                check.IsEnabled = true;
                cambiandoEstado = false;
            }
        }

        // EXPEDIENTE
        private void Expediente_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton &&
                boton.DataContext is PacienteVista paciente)
            {
                MessageBox.Show(
                    "Expediente de: " + paciente.NombreCompleto +
                    "\n\nAquí conectaremos el expediente clínico.",
                    "Expediente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        // EDITAR PACIENTE
        private async void Editar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton &&
                boton.DataContext is PacienteVista paciente)
            {
                EditarPacienteWindow ventana =
                    new EditarPacienteWindow(paciente);

                ventana.Owner = Window.GetWindow(this);

                // VALIDAR IDENTIDAD DUPLICADA
                // EXCLUYENDO AL PACIENTE QUE ESTAMOS EDITANDO
                ventana.IdentidadYaRegistrada = identidad =>
                    pacientes.Any(p =>
                        p.IdPaciente != paciente.IdPaciente &&
                        new string(
                            p.Identidad.Where(char.IsDigit).ToArray()
                        ) == identidad);

                bool? resultado = ventana.ShowDialog();

                if (resultado == true)
                {
                    try
                    {
                        // ACTUALIZAR TABLA DESDE SQL SERVER
                        await CargarPacientesAsync();

                        // MOSTRAR NOTIFICACIÓN
                        MostrarAviso(
                            "Paciente actualizado exitosamente.",
                            true);
                    }
                    catch (Exception ex)
                    {
                        MostrarAviso(
                            "No se pudo actualizar la tabla.",
                            false);

                        MessageBox.Show(
                            ex.Message,
                            "ClinicSync",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                else if (ventana.ErrorAlGuardar)
                {
                    MostrarAviso(
                        "Error al actualizar registro.",
                        false);
                }
            }
        }

    }
}