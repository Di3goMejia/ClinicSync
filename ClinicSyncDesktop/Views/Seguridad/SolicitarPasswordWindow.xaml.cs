using System.Windows;
using System.Windows.Input;

namespace ClinicSyncDesktop.Views.Seguridad;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.Windows.Window" />
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
public partial class SolicitarPasswordWindow : Window
{
    /// <summary>
    /// Gets the password.
    /// </summary>
    /// <value>
    /// The password.
    /// </value>
    public string Password => TxtPassword.Password;
    /// <summary>
    /// Initializes a new instance of the <see cref="SolicitarPasswordWindow"/> class.
    /// </summary>
    /// <param name="mensaje">The mensaje.</param>
    public SolicitarPasswordWindow(string mensaje)
    {
        InitializeComponent();
        TxtMensaje.Text = mensaje;
        TxtPassword.PasswordChanged += (_, _) => TxtError.Visibility = Visibility.Collapsed;
    }
    /// <summary>
    /// Handles the Click event of the Confirmar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
    /// <summary>
    /// Handles the Click event of the Cancelar control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    /// <summary>
    /// Handles the MouseLeftButtonDown event of the Encabezado control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
    private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
