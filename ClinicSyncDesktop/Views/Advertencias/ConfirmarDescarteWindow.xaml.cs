
using System.Windows;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ConfirmarDescarteWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmarDescarteWindow"/> class.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        public ConfirmarDescarteWindow(string mensaje = "¿Seguro que desea descartar este registro?")
        {
            InitializeComponent();
            txtMensaje.Text = mensaje;
        }

        // SÍ: DESCARTAR EL REGISTRO
        /// <summary>
        /// Handles the Click event of the Si control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Si_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = true;
        }

        // NO: REGRESAR AL FORMULARIO
        /// <summary>
        /// Handles the Click event of the No control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void No_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
