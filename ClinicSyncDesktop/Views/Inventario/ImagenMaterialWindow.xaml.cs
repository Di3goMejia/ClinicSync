using System.Windows;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views
{
    public partial class ImagenMaterialWindow : Window
    {
        public ImagenMaterialWindow(
            string nombreMaterial,
            BitmapImage imagen)
        {
            InitializeComponent();

            txtEncabezado.Text = nombreMaterial;
            imgMaterial.Source = imagen;
        }

        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}
