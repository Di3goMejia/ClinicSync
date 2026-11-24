using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Validations;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class CambiarContrasenaWindow : Window
    {
        /// <summary>
        /// The sincronizando
        /// </summary>
        private bool sincronizando;
        /// <summary>
        /// The guardando
        /// </summary>
        private bool guardando;

        /// <summary>
        /// The verde
        /// </summary>
        private static readonly Brush Verde =
            new SolidColorBrush(Color.FromRgb(32, 145, 70));

        /// <summary>
        /// The rojo
        /// </summary>
        private static readonly Brush Rojo =
            new SolidColorBrush(Color.FromRgb(221, 53, 78));

        /// <summary>
        /// The gris
        /// </summary>
        private static readonly Brush Gris =
            new SolidColorBrush(Color.FromRgb(145, 145, 145));

        /// <summary>
        /// Gets the error cambio.
        /// </summary>
        /// <value>
        /// The error cambio.
        /// </value>
        public string? ErrorCambio { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CambiarContrasenaWindow"/> class.
        /// </summary>
        public CambiarContrasenaWindow()
        {
            InitializeComponent();
            Validar();
        }

        /// <summary>
        /// Reglas the specified control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="cumple">if set to <c>true</c> [cumple].</param>
        /// <param name="texto">The texto.</param>
        private void Regla(
            TextBlock control,
            bool cumple,
            string texto)
        {
            control.Text = (cumple ? "✔ " : "✖ ") + texto;
            control.Foreground = cumple ? Verde : Gris;
        }

        // VALIDACIONES EN TIEMPO REAL
        /// <summary>
        /// Validars this instance.
        /// </summary>
        private void Validar()
        {
            if (BtnGuardar == null)
                return;

            string actual = Actual.Password;
            string nueva = Nueva.Password;
            string confirmar = Confirmacion.Password;

            bool longitud = nueva.Length >= 8;
            bool mayuscula = nueva.Any(char.IsUpper);
            bool minuscula = nueva.Any(char.IsLower);
            bool numero = nueva.Any(char.IsDigit);

            bool especial = nueva.Any(
                c => !char.IsLetterOrDigit(c)
                     && !char.IsWhiteSpace(c));

            bool coinciden =
                nueva.Length > 0 && nueva == confirmar;

            var resultadoValidacion = new ValidadorCambioContrasena().Validar(
                new EntradaCambioContrasena(actual, nueva, confirmar));

            Regla(ReglaLongitud, longitud,
                "Mínimo de 8 caracteres");

            Regla(ReglaMayuscula, mayuscula,
                "Al menos una letra mayúscula (A-Z)");

            Regla(ReglaMinuscula, minuscula,
                "Al menos una letra minúscula (a-z)");

            Regla(ReglaNumero, numero,
                "Al menos un dígito (0-9)");

            Regla(ReglaEspecial, especial,
                "Al menos un carácter especial");

            if (confirmar.Length == 0)
            {
                TextoCoincidencia.Text = "";
            }
            else if (coinciden)
            {
                TextoCoincidencia.Text =
                    "Las contraseñas coinciden.";
            }
            else
            {
                TextoCoincidencia.Text =
                    "Las contraseñas no coinciden.";
            }

            TextoCoincidencia.Foreground =
                coinciden ? Verde : Rojo;

            BtnGuardar.IsEnabled =
                !guardando &&
                resultadoValidacion.EsValido;
        }

        // VALIDAR MIENTRAS ESCRIBE
        /// <summary>
        /// Handles the PasswordChanged event of the Campos control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Campos_PasswordChanged(
            object sender,
            RoutedEventArgs e)
        {
            if (sincronizando)
                return;

            sincronizando = true;

            ActualVisible.Text = Actual.Password;
            NuevaVisible.Text = Nueva.Password;
            ConfirmacionVisible.Text = Confirmacion.Password;

            sincronizando = false;
            Validar();
        }

        /// <summary>
        /// Handles the TextChanged event of the Visible control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void Visible_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (sincronizando)
                return;

            sincronizando = true;

            Actual.Password = ActualVisible.Text;
            Nueva.Password = NuevaVisible.Text;
            Confirmacion.Password = ConfirmacionVisible.Text;

            sincronizando = false;
            Validar();
        }

        // MOSTRAR U OCULTAR CONTRASEÑAS
        /// <summary>
        /// Handles the Click event of the AlternarVisibilidad control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AlternarVisibilidad_Click(
            object sender,
            RoutedEventArgs e)
        {
            string? tag = (sender as Button)?.Tag as string;

            PasswordBox clave =
                tag == "actual" ? Actual :
                tag == "nueva" ? Nueva :
                Confirmacion;

            TextBox visible =
                tag == "actual" ? ActualVisible :
                tag == "nueva" ? NuevaVisible :
                ConfirmacionVisible;

            bool mostrar =
                clave.Visibility == Visibility.Visible;

            clave.Visibility = mostrar
                ? Visibility.Collapsed
                : Visibility.Visible;

            visible.Visibility = mostrar
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // CERRAR VENTANA
        /// <summary>
        /// Handles the Click event of the Cerrar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            SolicitarCerrar();
        }

        /// <summary>
        /// Solicitars the cerrar.
        /// </summary>
        private void SolicitarCerrar()
        {
            if (guardando)
                return;

            if (Actual.Password.Length == 0 &&
                Nueva.Password.Length == 0 &&
                Confirmacion.Password.Length == 0)
            {
                DialogResult = false;
                return;
            }

            var confirmar = new ConfirmarDescarteWindow(
                "¿Seguro que desea descartar este cambio?")
            {
                Owner = this
            };

            if (confirmar.ShowDialog() == true)
            {
                DialogResult = false;
            }
        }


        /// <summary>
        /// Raises the <see cref="E:System.Windows.Window.Closing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
        protected override void OnClosing(
            System.ComponentModel.CancelEventArgs e)
        {
            // Impedir cerrar mientras se guarda,
            // excepto cuando el cambio terminó correctamente.
            if (guardando && DialogResult != true)
            {
                e.Cancel = true;
            }

            base.OnClosing(e);
        }


        // GUARDAR CAMBIO DE CONTRASEÑA

        // GUARDAR CAMBIO DE CONTRASEÑA

        /// <summary>
        /// Handles the Click event of the Guardar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Guardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            Validar();

            if (!BtnGuardar.IsEnabled)
                return;

            guardando = true;
            BtnGuardar.IsEnabled = false;

            try
            {
                await ClienteApi.CambiarMiContrasenaAsync(
                    Actual.Password,
                    Nueva.Password,
                    Confirmacion.Password);

                // El servidor confirmó el cambio.
                guardando = false;

                // Cerrar la ventana indicando éxito.
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorCambio = ex.Message;

                PanelAvisos.Children.Clear();
                PanelAvisos.Visibility = Visibility.Visible;

                PanelAvisos.Children.Add(
                    new AvisoNotificacion(
                        ex.Message,
                        false));
            }
            finally
            {
                guardando = false;
                Validar();
            }
        }

    }
}