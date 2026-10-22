using ClinicSyncDesktop.Api;
using ClinicSyncDesktop.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ClinicSyncDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var store = new TokenStore();
            store.CargarRefresh();

            var api = new ClienteApi("http://localhost:5227", store);
            var auth = new ClienteAuth(api, store);

            Window ventana = store.RefreshToken is not null
                ? new MainWindow(api, auth, store)
                : new LoginWindow(auth, api, store);

            ventana.Show();
        }
    }

}
