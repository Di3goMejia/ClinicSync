using System.Windows;

namespace ClinicSyncDesktop.Views.Perfil._2FA
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class Activar2FAWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Activar2FAWindow"/> class.
        /// </summary>
        public Activar2FAWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the Cancelar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Handles the Click event of the Continuar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Continuar_Click(object sender, RoutedEventArgs e)
        {
            CapturaFacialWindow ventanaCaptura = new CapturaFacialWindow
            {
                Owner = this
            };

            bool? resultado = ventanaCaptura.ShowDialog();

            if (resultado == true)
            {
                DialogResult = true;
            }
        }
    }
}