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

        public async Task EnviarReactivacionAsync(string correo, string nombre, CancellationToken ct = default)
        {
            string remitente = _config["Gmail:Correo"] ?? throw new Exception("Falta configurar Gmail.");
            string password = _config["Gmail:Password"] ?? throw new Exception("Falta la clave de Gmail.");
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("ClinicSync", remitente));
            mensaje.To.Add(MailboxAddress.Parse(correo));
            mensaje.Subject = "Cuenta reactivada - ClinicSync";
            mensaje.Body = new TextPart("plain") { Text = $"Hola, {nombre}\n\nTu cuenta de ClinicSync ha sido reactivada por un administrador.\n\nClinicSync" };
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(remitente, password, ct);
            await smtp.SendAsync(mensaje, ct);
            await smtp.DisconnectAsync(true, ct);
        }

        public async Task EnviarDesactivacionAsync(
            string correo,
            string nombre,
            CancellationToken ct = default)
        {
            string remitente = _config["Gmail:Correo"]
                ?? throw new Exception("Falta configurar Gmail.");
            string password = _config["Gmail:Password"]
                ?? throw new Exception("Falta la clave de Gmail.");

            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("ClinicSync", remitente));
            mensaje.To.Add(MailboxAddress.Parse(correo));
            mensaje.Subject = "Cuenta desactivada - ClinicSync";
            mensaje.Body = new TextPart("plain")
            {
                Text = $"Hola, {nombre}\n\n" +
                       "Un administrador ha desactivado tu cuenta de ClinicSync.\n\n" +
                       "Si consideras que esto es un error, comunícate con el administrador del sistema.\n\n" +
                       "ClinicSync"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(remitente, password, ct);
            await smtp.SendAsync(mensaje, ct);
            await smtp.DisconnectAsync(true, ct);
        }

        public async Task EnviarActivacionAsync(
            string correo,
            string nombre,
            string token,
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

            mensaje.Subject = "Activacion de Cuenta - ClinicSync";

            mensaje.Body = new TextPart("plain")
            {
                Text =
                    $"Hola, {nombre}\n\n" +
                    "Un administrador te ha creado una cuenta en ClinicSync.\n\n" +
                    "Para ingresar, utiliza el siguiente código:\n\n" +
                    token + "\n\n" +
                    "Este código caduca en 1 día.\n\n" +
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
    }
}
