using System;
using System.Collections.Generic;
using System.Text;
using ClinicSyncServer.Common.Abstracciones;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace ClinicSyncServer.Infrastructure.Email
{
    public class GmailEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public GmailEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarRestablecimientoAsync(
            string correo,
            string codigo,
            CancellationToken ct = default)
        {
            string remitente = _config["Gmail:Correo"]
                ?? throw new Exception("Falta configurar Gmail.");

            string password = _config["Gmail:Password"]
                ?? throw new Exception("Falta la clave de Gmail.");

            var mensaje = new MimeMessage();

            mensaje.From.Add(
                new MailboxAddress("ClinicSync", remitente));

            mensaje.To.Add(MailboxAddress.Parse(correo));

            mensaje.Subject =
                "Código de recuperación - ClinicSync";

            mensaje.Body = new TextPart("plain")
            {
                Text =
                    "Hola,\n\n" +
                    "Solicitaste restablecer tu contraseña.\n\n" +
                    "Tu código de recuperación es:\n\n" +
                    codigo + "\n\n" +
                    "Este código caduca en 10 minutos.\n\n" +
                    "Si no solicitaste este cambio, " +
                    "ignora este mensaje.\n\n" +
                    "ClinicSync"
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls,
                ct);

            await smtp.AuthenticateAsync(
                remitente,
                password,
                ct);

            await smtp.SendAsync(mensaje, ct);

            await smtp.DisconnectAsync(true, ct);
        }

        public Task EnviarActivacionAsync(
            string correo,
            string nombre,
            string token,
            CancellationToken ct = default)
        {
            // Conservar aquí la implementación existente
            // de activación de cuentas cuando esté disponible.
            return Task.CompletedTask;
        }
    }
}
