using ClinicSyncDesktop.Api;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly ClienteAuth _auth;
        private readonly ClienteApi _api;
        private readonly TokenStore _store;

        public LoginWindow(ClienteAuth auth, ClienteApi api, TokenStore store)
        {
            InitializeComponent();
            _auth = auth;
            _api = api;
            _store = store;
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            BtnLogin.IsEnabled = false;
            try
            {
                var r = await _auth.LoginAsync(
                    TxtUsuario.Text.Trim(), 
                    TxtContrasena.Password
                );
                TxtContrasena.Clear();

                if (r.Requiere2fa)
                {
                    MessageBox.Show("Esta cuenta requiere segundo factor, que aún no está disponible.",
                                    "Inicio de sesión", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                new MainWindow(_api, _auth, _store).Show();
                Close();
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Message, "Inicio de sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("No se pudo conectar con el servidor.", "Error de conexión",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnLogin.IsEnabled = true;
            }
        }
    }
}
