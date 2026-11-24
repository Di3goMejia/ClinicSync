using System.Windows;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ImagenMaterialWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImagenMaterialWindow"/> class.
        /// </summary>
        /// <param name="nombreMaterial">The nombre material.</param>
        /// <param name="imagen">The imagen.</param>
        public ImagenMaterialWindow(
            string nombreMaterial,
            BitmapImage imagen)
        {
            InitializeComponent();

            txtEncabezado.Text = nombreMaterial;
            imgMaterial.Source = imagen;
        }

        /// <summary>
        /// Handles the Click event of the Cerrar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}
