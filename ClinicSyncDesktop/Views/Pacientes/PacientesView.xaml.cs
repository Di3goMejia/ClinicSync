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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class PacienteVista : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int IdPaciente { get; set; }

        /// <summary>
        /// Gets or sets the identidad.
        /// </summary>
        /// <value>
        /// The identidad.
        /// </value>
        public string Identidad { get; set; } = "";
        /// <summary>
        /// Gets or sets the nombres.
        /// </summary>
        /// <value>
        /// The nombres.
        /// </value>
        public string Nombres { get; set; } = "";
        /// <summary>
        /// Gets or sets the apellidos.
        /// </summary>
        /// <value>
        /// The apellidos.
        /// </value>
        public string Apellidos { get; set; } = "";
        /// <summary>
        /// Gets or sets the sexo.
        /// </summary>
        /// <value>
        /// The sexo.
        /// </value>
        public string Sexo { get; set; } = "";
        /// <summary>
        /// Gets or sets the fecha nacimiento.
        /// </summary>
        /// <value>
        /// The fecha nacimiento.
        /// </value>
        public DateTime FechaNacimiento { get; set; }
        /// <summary>
        /// Gets or sets the telefono.
        /// </summary>
        /// <value>
        /// The telefono.
        /// </value>
        public string Telefono { get; set; } = "";

        /// <summary>
        /// The activo
        /// </summary>
        private bool activo = true;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PacienteVista"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets the nombre completo.
        /// </summary>
        /// <value>
        /// The nombre completo.
        /// </value>
        public string NombreCompleto => Nombres + " " + Apellidos;

        /// <summary>
        /// Gets the edad.
        /// </summary>
        /// <value>
        /// The edad.
        /// </value>
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

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class PacienteApi
    {
        /// <summary>
        /// Gets or sets the identifier paciente.
        /// </summary>
        /// <value>
        /// The identifier paciente.
        /// </value>
        public int IdPaciente { get; set; }
        /// <summary>
        /// Gets or sets the identidad.
        /// </summary>
        /// <value>
        /// The identidad.
        /// </value>
        public string Identidad { get; set; } = "";
        /// <summary>
        /// Gets or sets the nombres.
        /// </summary>
        /// <value>
        /// The nombres.
        /// </value>
        public string Nombres { get; set; } = "";
        /// <summary>
        /// Gets or sets the apellidos.
        /// </summary>
        /// <value>
        /// The apellidos.
        /// </value>
        public string Apellidos { get; set; } = "";
        /// <summary>
        /// Gets or sets the genero.
        /// </summary>
        /// <value>
        /// The genero.
        /// </value>
        public string Genero { get; set; } = "";
        /// <summary>
        /// Gets or sets the fecha nacimiento.
        /// </summary>
        /// <value>
        /// The fecha nacimiento.
        /// </value>
        public DateTime FechaNacimiento { get; set; }
        /// <summary>
        /// Gets or sets the telefono.
        /// </summary>
        /// <value>
        /// The telefono.
        /// </value>
        public string Telefono { get; set; } = "";
        /// <summary>
        /// Gets or sets the fecha registro.
        /// </summary>
        /// <value>
        /// The fecha registro.
        /// </value>
        public DateTime FechaRegistro { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PacienteApi"/> is activo.
        /// </summary>
        /// <value>
        ///   <c>true</c> if activo; otherwise, <c>false</c>.
        /// </value>
        public bool Activo { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class PacientesView : UserControl
    {
        /// <summary>
        /// The pacientes
        /// </summary>
        private readonly ObservableCollection<PacienteVista> pacientes = new();
        /// <summary>
        /// The vista pacientes
        /// </summary>
        private readonly ICollectionView vistaPacientes;

        /// <summary>
        /// The cargando pacientes
        /// </summary>
        private bool cargandoPacientes = false;
        /// <summary>
        /// The cambiando estado
        /// </summary>
        private bool cambiandoEstado = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PacientesView"/> class.
        /// </summary>
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
        /// <summary>
        /// Mostrars the aviso.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="exitoso">if set to <c>true</c> [exitoso].</param>
        private void MostrarAviso(
            string mensaje,
            bool exitoso)
        {
            PanelAvisos.Children.Clear();

            AvisoNotificacion aviso =
                new AvisoNotificacion(mensaje, exitoso);

            PanelAvisos.Children.Add(aviso);
        }

        // CARGAR PACIENTES
        /// <summary>
        /// Handles the Loaded event of the PacientesView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Cargars the pacientes asynchronous.
        /// </summary>
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

        /// <summary>
        /// Filtrars the paciente.
        /// </summary>
        /// <param name="objeto">The objeto.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handles the TextChanged event of the txtBuscar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void txtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            // Actualizar los resultados mientras escribimos.
            vistaPacientes?.Refresh();
        }


        // AGREGAR PACIENTE
        /// <summary>
        /// Handles the Click event of the btnAgregar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the Click event of the Estado control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        // ABRIR EXPEDIENTE DEL PACIENTE
        /// <summary>
        /// Handles the Click event of the Expediente control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Expediente_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton &&
                boton.DataContext is PacienteVista paciente)
            {
                if (Window.GetWindow(this) is ClinicSync.Views.MenuLateral menu)
                {
                    menu.AbrirExpediente(paciente.IdPaciente);
                }
            }
        }

        // EDITAR PACIENTE
        /// <summary>
        /// Handles the Click event of the Editar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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