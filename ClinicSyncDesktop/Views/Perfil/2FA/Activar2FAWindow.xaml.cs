using System.Windows;

namespace ClinicSyncDesktop.Views.Perfil._2FA
{
    public partial class Activar2FAWindow : Window
    {
        public Activar2FAWindow()
        {
            InitializeComponent();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

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