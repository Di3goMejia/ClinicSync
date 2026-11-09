using System.Windows;

namespace ClinicSyncDesktop.Views.Seguridad;

public partial class SolicitarPasswordWindow : Window
{
    public string Password => TxtPassword.Password;
    public SolicitarPasswordWindow(string mensaje)
    {
        InitializeComponent();
        TxtMensaje.Text = mensaje;
    }
    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Password)) { 
            MessageBox.Show("La contraseña es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning); 
            return; 
        }
        
        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
