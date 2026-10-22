using ClinicSyncDesktop.Api;
using ClinicSyncDesktop.Views;
using System.Windows;

namespace ClinicSyncDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ClienteApi _api;
        private readonly ClienteAuth _auth;
        private readonly TokenStore _tokens;

        public MainWindow(ClienteApi api, ClienteAuth auth, TokenStore tokens)
        {
            InitializeComponent();

            _api = api;
            _auth = auth;
            _tokens = tokens;
        }

        private void VolverALogin(string? aviso)
        {
            if (aviso is not null)
                MessageBox.Show(aviso, "Sesión", MessageBoxButton.OK, MessageBoxImage.Information);

            new LoginWindow(_auth, _api, _tokens).Show();
            Close();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await _auth.CerrarSesionAsync();
            VolverALogin(null);
        }
    }
}
