
using ClinicSyncDesktop.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    public partial class ExpedientesView : UserControl
    {
        private readonly int idPaciente;
        private bool cargandoExpediente = false;

        public ExpedientesView(int idPaciente)
        {
            InitializeComponent();

            this.idPaciente = idPaciente;

            Loaded += ExpedientesView_Loaded;
        }

        // MODELO DEL EXPEDIENTE
        public class ExpedienteApi
        {
            public int IdPaciente { get; set; }
            public string? Notas { get; set; }
        }

        // MODELO DE LAS VISITAS
        public class VisitaApi
        {
            public int IdVisita { get; set; }
            public int IdPaciente { get; set; }
            public int IdEmpleado { get; set; }
            public DateTime Fecha { get; set; }
            public string Motivo { get; set; } = "";
            public string? Notas { get; set; }
        }

        // MOSTRAR AVISOS DE ÉXITO Y ERROR
        private void MostrarAviso(
            string mensaje,
            bool exitoso)
        {
            PanelAvisos.Children.Clear();

            AvisoPaciente aviso = new AvisoPaciente(
                mensaje,
                exitoso);

            PanelAvisos.Visibility = Visibility.Visible;

            PanelAvisos.Children.Add(aviso);
        }

        // CARGAR EXPEDIENTE AL ABRIR LA VISTA
        private async void ExpedientesView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (cargandoExpediente)
                return;

            try
            {
                await CargarExpedienteAsync();
            }
            catch (Exception)
            {
                txtEstado.Text =
                    "Error al cargar el expediente.";

                MostrarAviso(
                    "Error al cargar el expediente.",
                    false);
            }
        }

        // CONSULTAR DATOS DEL PACIENTE Y EXPEDIENTE
        private async Task CargarExpedienteAsync()
        {
            if (cargandoExpediente)
                return;

            cargandoExpediente = true;

            txtEstado.Text = "Cargando expediente...";

            try
            {
                // CONSULTAR PACIENTES
                PacienteApi[] pacientes =
                    await ClienteApi
                        .ObtenerAutenticadoAsync<PacienteApi[]>(
                            "api/pacientes");

                // BUSCAR PACIENTE
                PacienteApi? paciente =
                    pacientes.FirstOrDefault(p =>
                        p.IdPaciente == idPaciente);

                if (paciente == null)
                {
                    txtNombre.Text =
                        "Paciente no encontrado";

                    txtEstado.Text =
                        "No se encontró el paciente seleccionado.";

                    return;
                }

                // MOSTRAR DATOS DEL PACIENTE
                txtNombre.Text =
                    paciente.Nombres + " " +
                    paciente.Apellidos;

                txtSexo.Text = paciente.Genero;

                txtTelefono.Text = paciente.Telefono;

                // CALCULAR EDAD
                DateTime hoy = DateTime.Today;

                int edad =
                    hoy.Year -
                    paciente.FechaNacimiento.Year;

                if (paciente.FechaNacimiento.Date >
                    hoy.AddYears(-edad))
                {
                    edad--;
                }

                txtEdad.Text = edad.ToString();

                // CONSULTAR EXPEDIENTE
                ExpedienteApi expediente =
                    await ClienteApi
                        .ObtenerAutenticadoAsync<ExpedienteApi>(
                            $"api/expedientes/paciente/{idPaciente}");

                txtNotasExpediente.Text =
                    expediente?.Notas ?? "";

                // CONSULTAR VISITAS
                VisitaApi[] visitas =
                    await ClienteApi
                        .ObtenerAutenticadoAsync<VisitaApi[]>(
                            $"api/expedientes/paciente/{idPaciente}/visitas");

                // ORDENAR VISITAS
                dgVisitas.ItemsSource =
                    visitas
                        .OrderByDescending(v => v.Fecha)
                        .ToList();

                // MOSTRAR TOTAL
                if (visitas.Length == 0)
                {
                    txtEstado.Text =
                        "Este paciente todavía no tiene visitas registradas.";
                }
                else
                {
                    txtEstado.Text =
                        $"Total de visitas: {visitas.Length}";
                }

                // ARCHIVOS PENDIENTES
                PanelArchivos.Children.Clear();
            }
            finally
            {
                cargandoExpediente = false;
            }
        }

        // VOLVER AL MÓDULO PACIENTES
        private void Volver_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (Window.GetWindow(this)
                is ClinicSync.Views.MenuLateral menu)
            {
                menu.VolverAPacientes();
            }
        }

        // AGREGAR NUEVA VISITA
        private async void AgregarVisita_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                AgregarVisitaWindow ventana =
                    new AgregarVisitaWindow(idPaciente);

                ventana.Owner = Window.GetWindow(this);

                bool? resultado = ventana.ShowDialog();

                if (resultado == true)
                {
                    await CargarExpedienteAsync();

                    MostrarAviso(
                        "Visita registrada exitosamente.",
                        true);
                }
            }
            catch (Exception)
            {
                MostrarAviso(
                    "Error al agregar registro.",
                    false);
            }
        }

        // EDITAR VISITA EXISTENTE
        private async void EditarVisita_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton &&
                boton.DataContext is VisitaApi visita)
            {
                try
                {
                    // ABRIR LA VISITA SELECCIONADA
                    EditarVisitaWindow ventana =
                        new EditarVisitaWindow(visita.IdVisita);

                    ventana.Owner = Window.GetWindow(this);

                    bool? resultado = ventana.ShowDialog();

                    // SI SE GUARDARON LOS CAMBIOS
                    if (resultado == true)
                    {
                        // ACTUALIZAR LISTA DE VISITAS
                        await CargarExpedienteAsync();

                        // MOSTRAR AVISO
                        MostrarAviso(
                            "Visita actualizada exitosamente.",
                            true);
                    }
                }
                catch (Exception)
                {
                    MostrarAviso(
                        "Error al editar la visita.",
                        false);
                }
            }
        }

        // SUBIR ARCHIVO
        private void SubirArchivo_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBox.Show(
                "La carga de archivos se conectará " +
                "próximamente con ClinicSyncServer.",
                "Archivos del expediente",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
