using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    public partial class SeccionGenericaView : UserControl
    {
        public SeccionGenericaView()
        {
            InitializeComponent();
        }

        public SeccionGenericaView(string nombreSeccion) : this()
        {
            ConfigurarSeccion(nombreSeccion);
        }

        public void ConfigurarSeccion(string nombreSeccion)
        {
            TxtTituloBarra.Text = nombreSeccion.ToUpper();
            TxtTituloSeccion.Text = $"Sección de {nombreSeccion}";
            TxtDescripcionSeccion.Text = $"El módulo de {nombreSeccion} se integra de forma transparente en el diseño principal reutilizable sin necesidad de crear ventanas adicionales.";
        }
    }
}
