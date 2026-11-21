using System.Windows;
using System.Windows.Input;

namespace ClinicSyncDesktop.Views.Seguridad;

public partial class SolicitarPasswordWindow : Window
{
    public string Password => TxtPassword.Password;
    public SolicitarPasswordWindow(string mensaje)
    {
        InitializeComponent();
        TxtMensaje.Text = mensaje;
        TxtPassword.PasswordChanged += (_, _) => TxtError.Visibility = Visibility.Collapsed;
    }
    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            TxtError.Text = "La contraseña es obligatoria.";
            TxtError.Visibility = Visibility.Visible;
            return;
        }
        DialogResult = true;
    }
    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
