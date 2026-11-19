using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Validations;
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
            var resultado = new ResultadoValidacion();
            resultado.Campo<string?>("Correo", correo)
                .Requerido("El correo electrónico es obligatorio.")
                .CorreoValido("Ingrese un correo electrónico válido.");
            if (resultado.EsValido) return true;
            MostrarError(TxtCorreo, LblErrorCorreo, resultado.Errores[0].Mensaje);
            return false;
        }

        // ============================================
        // VALIDACIÓN DEL CÓDIGO
        // ============================================

        private bool ValidarCodigo()
        {
            string codigo = TxtCodigo.Text.Trim();

            LimpiarError(TxtCodigo, LblErrorCodigo);
            var resultado = new ResultadoValidacion();
            resultado.Campo<string?>("Codigo", codigo)
                .Requerido("El código de recuperación es obligatorio.")
                .CodigoNumerico(6, "El código debe tener exactamente 6 dígitos.");
            if (resultado.EsValido) return true;
            MostrarError(TxtCodigo, LblErrorCodigo, resultado.Errores[0].Mensaje);
            return false;
        }

        // ============================================
        // VALIDACIÓN DE NUEVA CONTRASEÑA
        // ============================================

        private bool ValidarNuevaPassword()
        {
            string password = TxtNuevaPassword.Password;

            LimpiarError(TxtNuevaPassword, LblErrorPassword);

            var resultado = new ValidadorNuevaContrasena().ValidarNueva(password);
            if (resultado.EsValido)
                return true;
            MostrarError(TxtNuevaPassword, LblErrorPassword, resultado.Errores[0].Mensaje);
            return false;
        }

        // ============================================
        // VALIDACIÓN DE CONFIRMAR CONTRASEÑA
        // ============================================

        private bool ValidarConfirmarPassword()
        {
            string password = TxtNuevaPassword.Password;
            string confirmar = TxtConfirmarPassword.Password;

            LimpiarError(TxtConfirmarPassword, LblErrorConfirmar);

            var resultado = new ValidadorNuevaContrasena().ValidarConfirmacion(
                new EntradaNuevaContrasena(password, confirmar));
            if (resultado.EsValido)
                return true;
            MostrarError(TxtConfirmarPassword, LblErrorConfirmar, resultado.Errores[0].Mensaje);
            return false;
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


