using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Validations;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    public partial class ActivacionCuentaView : Window
    {
        private bool enviando = false;

        private readonly Brush colorError = Brushes.Red;
        private readonly Brush colorNormal = Brushes.LightGray;

        public ActivacionCuentaView()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(TxtToken, OnPastingToken);
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
        // VALIDACIÓN DEL TOKEN / CÓDIGO
        // ============================================

        private bool ValidarTokenLocal()
        {
            string token = TxtToken.Text.Trim();

            LimpiarError(TxtToken, LblErrorToken);
            var resultado = new ResultadoValidacion();
            resultado.Campo<string?>("Token", token)
                .Requerido("El código de activación es obligatorio.")
                .CodigoNumerico(6, "El código debe tener exactamente 6 dígitos.");
            if (resultado.EsValido) return true;
            MostrarError(TxtToken, LblErrorToken, resultado.Errores[0].Mensaje);
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
        // BOTÓN VALIDAR TOKEN (PASO 1)
        // ============================================

        private async void BtnValidarToken_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarMensajeGeneral();

            if (!ValidarTokenLocal())
                return;

            if (enviando)
                return;

            enviando = true;
            BtnValidarToken.IsEnabled = false;

            try
            {
                await ClienteApi.ValidarTokenActivacionAsync(
                    TxtToken.Text.Trim());

                PanelToken.Visibility = Visibility.Collapsed;
                PanelNuevaPassword.Visibility = Visibility.Visible;

                TxtNuevaPassword.Clear();
                TxtConfirmarPassword.Clear();
                TxtNuevaPassword.Focus();
            }
            catch (HttpRequestException ex)
            {
                MostrarMensajeGeneral(
                    "No se pudo conectar con el servidor: " + ex.Message);
            }
            catch (Exception ex)
            {
                MostrarError(
                    TxtToken,
                    LblErrorToken,
                    ex.Message);
            }
            finally
            {
                enviando = false;
                BtnValidarToken.IsEnabled = true;
            }
        }

        // ============================================
        // BOTÓN ACTIVAR CUENTA (PASO 2)
        // ============================================

        private async void BtnActivarCuenta_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarMensajeGeneral();

            bool passwordValida = ValidarNuevaPassword();
            bool confirmacionValida = ValidarConfirmarPassword();

            if (!passwordValida || !confirmacionValida)
                return;

            if (enviando)
                return;

            enviando = true;
            BtnActivarCuenta.IsEnabled = false;

            try
            {
                await ClienteApi.ActivarCuentaAsync(
                    TxtToken.Text.Trim(),
                    TxtNuevaPassword.Password,
                    TxtConfirmarPassword.Password);

                PanelNuevaPassword.Visibility = Visibility.Collapsed;
                TxtNuevaPassword.Clear();
                TxtConfirmarPassword.Clear();

                MostrarMensajeGeneral(
                    "¡Cuenta activada exitosamente! " +
                    "Ya puede regresar e iniciar sesión.",
                    false);
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message;

                // Errores relacionados con el token expirado o inválido
                if (mensaje.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                    mensaje.Contains("código", StringComparison.OrdinalIgnoreCase) ||
                    mensaje.Contains("expir", StringComparison.OrdinalIgnoreCase) ||
                    mensaje.Contains("caduc", StringComparison.OrdinalIgnoreCase) ||
                    mensaje.Contains("enlace", StringComparison.OrdinalIgnoreCase))
                {
                    PanelNuevaPassword.Visibility = Visibility.Collapsed;
                    PanelToken.Visibility = Visibility.Visible;

                    MostrarError(
                        TxtToken,
                        LblErrorToken,
                        "El código es incorrecto o ha caducado. Solicite uno nuevo a un administrador.");

                    TxtToken.Focus();
                }
                else
                {
                    MostrarMensajeGeneral(
                        "No se pudo activar la cuenta: " + mensaje);
                }
            }
            finally
            {
                enviando = false;
                BtnActivarCuenta.IsEnabled = true;
            }
        }

        // ============================================
        // MANEJO DEL PORTAPAPELES Y ENTRADA DE TEXTO
        // ============================================

        private void BtnPegar_Click(object sender, RoutedEventArgs e)
        {
            PegarCodigoDesdePortapapeles();
        }

        private void PegarCodigoDesdePortapapeles()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string texto = Clipboard.GetText();
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        string limpio = Regex.Replace(texto.Trim(), @"[^\d]", "");
                        if (limpio.Length > 6)
                            limpio = limpio.Substring(0, 6);

                        TxtToken.Text = limpio;
                        TxtToken.CaretIndex = TxtToken.Text.Length;
                        TxtToken.Focus();
                    }
                }
            }
            catch
            {
                // Ignorar si el portapapeles no está disponible
            }
        }

        private void OnPastingToken(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string? texto = e.DataObject.GetData(DataFormats.Text) as string;
                if (!string.IsNullOrEmpty(texto))
                {
                    string limpio = Regex.Replace(texto.Trim(), @"[^\d]", "");
                    if (limpio.Length > 6)
                        limpio = limpio.Substring(0, 6);

                    TxtToken.Text = limpio;
                    TxtToken.CaretIndex = TxtToken.Text.Length;
                    e.CancelCommand();
                    e.Handled = true;
                }
            }
        }

        private void TxtToken_PreviewTextInput(
            object sender,
            TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void TxtToken_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            LimpiarError(
                TxtToken,
                LblErrorToken);

            string actual = TxtToken.Text;
            string limpio = Regex.Replace(actual, @"[^\d]", "");
            if (limpio.Length > 6)
                limpio = limpio.Substring(0, 6);

            if (actual != limpio)
            {
                TxtToken.Text = limpio;
                TxtToken.CaretIndex = limpio.Length;
            }
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
