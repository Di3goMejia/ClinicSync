
using ClinicSyncDesktop.Data;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;

namespace ClinicSyncDesktop.Views
{
    public partial class RecuperacionContrasenaView : Window
    {
        private bool enviando = false;

        private readonly Brush colorError =
            Brushes.Red;

        private readonly Brush colorNormal =
            Brushes.LightGray;

        public RecuperacionContrasenaView()
        {
            InitializeComponent();
        }

        // ============================================
        // FUNCIONES GENERALES PARA MOSTRAR ERRORES
        // ============================================

        private void MostrarError(
            Control campo,
            TextBlock etiqueta,
            string mensaje)
        {
            campo.BorderBrush = colorError;
            campo.BorderThickness = new Thickness(2);

            etiqueta.Text = mensaje;
            etiqueta.Visibility = Visibility.Visible;
        }

        private void LimpiarError(
            Control campo,
            TextBlock etiqueta)
        {
            campo.BorderBrush = colorNormal;
            campo.BorderThickness = new Thickness(1);

            etiqueta.Text = "";
            etiqueta.Visibility = Visibility.Collapsed;
        }

        private void MostrarMensajeGeneral(
            string mensaje,
            bool esError = true)
        {
            LblMensajeGeneral.Text = mensaje;

            LblMensajeGeneral.Foreground =
                esError ? Brushes.DarkRed : Brushes.DarkGreen;

            LblMensajeGeneral.Visibility =
                Visibility.Visible;
        }

        private void LimpiarMensajeGeneral()
        {
            LblMensajeGeneral.Text = "";
            LblMensajeGeneral.Visibility =
                Visibility.Collapsed;
        }

        // ============================================
        // VALIDACIÓN DEL CORREO
        // ============================================

        private bool ValidarCorreo()
        {
            string correo = TxtCorreo.Text.Trim();

            LimpiarError(TxtCorreo, LblErrorCorreo);

            if (string.IsNullOrWhiteSpace(correo))
            {
                MostrarError(
                    TxtCorreo,
                    LblErrorCorreo,
                    "El correo electrónico es obligatorio.");

                return false;
            }

            if (!Regex.IsMatch(
                correo,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MostrarError(
                    TxtCorreo,
                    LblErrorCorreo,
                    "Ingrese un correo electrónico válido.");

                return false;
            }

            return true;
        }

        // ============================================
        // VALIDACIÓN DEL CÓDIGO
        // ============================================

        private bool ValidarCodigo()
        {
            string codigo = TxtCodigo.Text.Trim();

            LimpiarError(TxtCodigo, LblErrorCodigo);

            if (string.IsNullOrWhiteSpace(codigo))
            {
                MostrarError(
                    TxtCodigo,
                    LblErrorCodigo,
                    "El código de recuperación es obligatorio.");

                return false;
            }

            if (!Regex.IsMatch(codigo, @"^[0-9]{6}$"))
            {
                MostrarError(
                    TxtCodigo,
                    LblErrorCodigo,
                    "El código debe tener exactamente 6 dígitos.");

                return false;
            }

            return true;
        }

        // ============================================
        // VALIDACIÓN DE NUEVA CONTRASEÑA
        // ============================================

        private bool ValidarNuevaPassword()
        {
            string password = TxtNuevaPassword.Password;

            LimpiarError(
                TxtNuevaPassword,
                LblErrorPassword);

            if (string.IsNullOrWhiteSpace(password))
            {
                MostrarError(
                    TxtNuevaPassword,
                    LblErrorPassword,
                    "La nueva contraseña es obligatoria.");

                return false;
            }

            if (password.Length < 8)
            {
                MostrarError(
                    TxtNuevaPassword,
                    LblErrorPassword,
                    "La contraseña debe tener 8 caracteres o más.");

                return false;
            }

            if (!password.Any(char.IsLetter) ||
                !password.Any(char.IsDigit))
            {
                MostrarError(
                    TxtNuevaPassword,
                    LblErrorPassword,
                    "La contraseña debe contener letras y números.");

                return false;
            }

            return true;
        }

        // ============================================
        // VALIDACIÓN DE CONFIRMAR CONTRASEÑA
        // ============================================

        private bool ValidarConfirmarPassword()
        {
            string password = TxtNuevaPassword.Password;
            string confirmar = TxtConfirmarPassword.Password;

            LimpiarError(
                TxtConfirmarPassword,
                LblErrorConfirmar);

            if (string.IsNullOrWhiteSpace(confirmar))
            {
                MostrarError(
                    TxtConfirmarPassword,
                    LblErrorConfirmar,
                    "Debe confirmar la contraseña.");

                return false;
            }

            if (password != confirmar)
            {
                MostrarError(
                    TxtConfirmarPassword,
                    LblErrorConfirmar,
                    "Las contraseñas no coinciden.");

                return false;
            }

            return true;
        }

        // ============================================
        // BOTÓN ENVIAR CÓDIGO
        // ============================================

        private async void BtnEnviarCodigo_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarMensajeGeneral();

            if (!ValidarCorreo())
                return;

            await EnviarCodigoAsync();
        }

        private async Task EnviarCodigoAsync()
        {
            if (enviando)
                return;

            enviando = true;

            BtnEnviarCodigo.IsEnabled = false;
            BtnReenviar.IsEnabled = false;

            try
            {
                await ClienteApi.SolicitarCodigoAsync(
                    TxtCorreo.Text.Trim());

                PanelCorreo.Visibility =
                    Visibility.Collapsed;

                PanelCodigo.Visibility =
                    Visibility.Visible;

                PanelNuevaPassword.Visibility =
                    Visibility.Collapsed;

                TxtCodigo.Clear();

                MostrarMensajeGeneral(
                    "Si el correo está registrado, " +
                    "recibirá un código de recuperación.",
                    false);

                TxtCodigo.Focus();
            }
            catch (Exception ex)
            {
                MostrarMensajeGeneral(
                    "Error: " + ex.Message);
            }
            finally
            {
                enviando = false;

                BtnEnviarCodigo.IsEnabled = true;
                BtnReenviar.IsEnabled = true;
            }
        }

        // ============================================
        // BOTÓN CONTINUAR
        // ============================================

        private void BtnContinuar_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarMensajeGeneral();

            if (!ValidarCodigo())
                return;

            PanelCodigo.Visibility =
                Visibility.Collapsed;

            PanelNuevaPassword.Visibility =
                Visibility.Visible;

            TxtNuevaPassword.Focus();
        }

        // ============================================
        // BOTÓN GUARDAR CONTRASEÑA
        // ============================================

        private async void BtnGuardarPassword_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarMensajeGeneral();

            // Ejecutar las dos validaciones.
            // No usar && directamente porque
            // queremos mostrar ambos errores.
            bool passwordValida =
                ValidarNuevaPassword();

            bool confirmacionValida =
                ValidarConfirmarPassword();

            if (!passwordValida || !confirmacionValida)
                return;

            BtnGuardarPassword.IsEnabled = false;

            try
            {
                await ClienteApi.RestablecerPasswordAsync(
                    TxtCorreo.Text.Trim(),
                    TxtCodigo.Text.Trim(),
                    TxtNuevaPassword.Password,
                    TxtConfirmarPassword.Password);

                PanelNuevaPassword.Visibility =
                    Visibility.Collapsed;

                TxtNuevaPassword.Clear();
                TxtConfirmarPassword.Clear();

                MostrarMensajeGeneral(
                    "¡Contraseña restablecida correctamente! " +
                    "Puede regresar e iniciar sesión.",
                    false);
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message;

                // Errores relacionados con el código
                if (mensaje.Contains("código",
                        StringComparison.OrdinalIgnoreCase) ||
                    mensaje.Contains("token",
                        StringComparison.OrdinalIgnoreCase) ||
                    mensaje.Contains("expir",
                        StringComparison.OrdinalIgnoreCase) ||
                    mensaje.Contains("caduc",
                        StringComparison.OrdinalIgnoreCase) ||
                    mensaje.Contains("enlace",
                        StringComparison.OrdinalIgnoreCase))
                {
                    PanelNuevaPassword.Visibility =
                        Visibility.Collapsed;

                    PanelCodigo.Visibility =
                        Visibility.Visible;

                    MostrarError(
                        TxtCodigo,
                        LblErrorCodigo,
                        "El código es incorrecto o ha caducado. " +
                        "Solicite uno nuevo si es necesario.");

                    TxtCodigo.Focus();
                }
                else
                {
                    MostrarMensajeGeneral(
                        "No se pudo restablecer la contraseña. " +
                        mensaje);
                }
            }
            finally
            {
                BtnGuardarPassword.IsEnabled = true;
            }
        }

        // ============================================
        // BOTÓN REENVIAR CÓDIGO
        // ============================================

        private async void BtnReenviar_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarMensajeGeneral();

            if (!ValidarCorreo())
            {
                PanelCodigo.Visibility =
                    Visibility.Collapsed;

                PanelCorreo.Visibility =
                    Visibility.Visible;

                return;
            }

            await EnviarCodigoAsync();
        }

        // ============================================
        // PERMITIR SOLO NÚMEROS
        // ============================================

        private void TxtCodigo_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(
                e.Text,
                @"^[0-9]+$");
        }

        // ============================================
        // LIMPIAR ERRORES AL ESCRIBIR
        // ============================================

        private void TxtCorreo_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            LimpiarError(
                TxtCorreo,
                LblErrorCorreo);
        }

        private void TxtCodigo_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            LimpiarError(
                TxtCodigo,
                LblErrorCodigo);
        }

        private void TxtNuevaPassword_PasswordChanged(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarError(
                TxtNuevaPassword,
                LblErrorPassword);

            LimpiarError(
                TxtConfirmarPassword,
                LblErrorConfirmar);
        }

        private void TxtConfirmarPassword_PasswordChanged(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarError(
                TxtConfirmarPassword,
                LblErrorConfirmar);
        }

        // ============================================
        // BOTÓN REGRESAR
        // ============================================

        private void BtnRegresar_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}


