
using ClinicSyncDesktop.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ClinicSyncDesktop.Services
{
    public sealed class ReporteFinancieroPdfService
    {
        private static readonly XColor ColorPrincipal =
            XColor.FromArgb(22, 143, 167);

        private static readonly XColor ColorTexto =
            XColor.FromArgb(23, 70, 90);

        private static readonly XColor ColorFondo =
            XColor.FromArgb(235, 243, 242);

        private static readonly XColor ColorIngreso =
            XColor.FromArgb(24, 122, 61);

        private static readonly XColor ColorEgreso =
            XColor.FromArgb(179, 38, 30);

        public void GenerarReporte(
            string rutaArchivo,
            IReadOnlyList<MovimientoResumen> movimientos,
            string periodo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
                throw new ArgumentException(
                    "La ruta del archivo no es válida.");

            if (movimientos == null)
                throw new ArgumentNullException(nameof(movimientos));

            var datos = movimientos
                .OrderByDescending(m => m.Fecha)
                .ToList();

            decimal ingresos = datos
                .Where(m => string.Equals(
                    m.Tipo, "INGRESO",
                    StringComparison.OrdinalIgnoreCase))
                .Sum(m => m.Monto);

            decimal egresos = datos
                .Where(m => string.Equals(
                    m.Tipo, "EGRESO",
                    StringComparison.OrdinalIgnoreCase))
                .Sum(m => m.Monto);

            decimal balance = ingresos - egresos;

            using PdfDocument documento = new();

            documento.Info.Title = "Reporte financiero";
            documento.Info.Author = "Assar Dental";
            documento.Info.Subject =
                "Resumen de movimientos financieros";

            XFont fuenteTitulo =
                new("Arial", 20, XFontStyleEx.Bold);

            XFont fuenteSubtitulo =
                new("Arial", 10, XFontStyleEx.Regular);

            XFont fuenteSeccion =
                new("Arial", 11, XFontStyleEx.Bold);

            XFont fuenteNormal =
                new("Arial", 8, XFontStyleEx.Regular);

            XFont fuenteNegrita =
                new("Arial", 8, XFontStyleEx.Bold);

            XFont fuenteResumen =
                new("Arial", 12, XFontStyleEx.Bold);

            XGraphics gfx = null!;
            PdfPage pagina = null!;
            double posicionY = 0;

            void CrearPagina()
            {
                if (gfx != null)
                    gfx.Dispose();

                pagina = documento.AddPage();
                pagina.Size = PdfSharp.PageSize.A4;
                pagina.Orientation =
                    PdfSharp.PageOrientation.Landscape;

                gfx = XGraphics.FromPdfPage(pagina);

                gfx.DrawRectangle(
                    new XSolidBrush(ColorPrincipal),
                    0, 0, pagina.Width.Point, 9);

                gfx.DrawString(
                    "ASSAR DENTAL",
                    fuenteTitulo,
                    new XSolidBrush(ColorTexto),
                    new XRect(35, 25, 400, 30),
                    XStringFormats.CenterLeft);

                gfx.DrawString(
                    "REPORTE FINANCIERO",
                    fuenteSubtitulo,
                    new XSolidBrush(ColorTexto),
                    new XRect(35, 57, 400, 18),
                    XStringFormats.CenterLeft);

                gfx.DrawString(
                    "Generado: " +
                    DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    fuenteNormal,
                    XBrushes.Gray,
                    new XRect(550, 32, 250, 18),
                    XStringFormats.CenterRight);

                gfx.DrawString(
                    "Período: " + periodo,
                    fuenteNormal,
                    XBrushes.Gray,
                    new XRect(550, 52, 250, 18),
                    XStringFormats.CenterRight);

                posicionY = 95;
            }

            CrearPagina();

            // RESUMEN FINANCIERO
            gfx.DrawString(
                "Resumen financiero",
                fuenteSeccion,
                new XSolidBrush(ColorTexto),
                35, posicionY);

            posicionY += 15;

            double anchoTarjeta = 245;
            double altoTarjeta = 55;
            double separacion = 12;

            DibujarTarjeta(
                gfx, 35, posicionY,
                anchoTarjeta, altoTarjeta,
                "TOTAL INGRESOS",
                ingresos, ColorIngreso,
                fuenteNormal, fuenteResumen);

            DibujarTarjeta(
                gfx, 35 + anchoTarjeta + separacion,
                posicionY, anchoTarjeta, altoTarjeta,
                "TOTAL EGRESOS",
                egresos, ColorEgreso,
                fuenteNormal, fuenteResumen);

            DibujarTarjeta(
                gfx, 35 + (anchoTarjeta + separacion) * 2,
                posicionY, anchoTarjeta, altoTarjeta,
                "BALANCE NETO",
                balance, ColorTexto,
                fuenteNormal, fuenteResumen);

            posicionY += altoTarjeta + 25;

            // TABLA DE MOVIMIENTOS
            gfx.DrawString(
                "Detalle de movimientos",
                fuenteSeccion,
                new XSolidBrush(ColorTexto),
                35, posicionY);

            posicionY += 12;

            double[] anchosColumnas =
            {
                85, 115, 270, 145, 125
            };

            string[] encabezados =
            {
                "Tipo",
                "Fecha",
                "Concepto",
                "Medio de pago",
                "Monto"
            };

            double anchoTotal = anchosColumnas.Sum();

            gfx.DrawRectangle(
                new XSolidBrush(ColorPrincipal),
                35, posicionY, anchoTotal, 25);

            double posicionX = 35;

            for (int i = 0; i < encabezados.Length; i++)
            {
                gfx.DrawString(
                    encabezados[i],
                    fuenteNegrita,
                    XBrushes.White,
                    new XRect(
                        posicionX + 5,
                        posicionY,
                        anchosColumnas[i] - 10,
                        25),
                    XStringFormats.CenterLeft);

                posicionX += anchosColumnas[i];
            }

            posicionY += 25;

            foreach (MovimientoResumen movimiento in datos)
            {
                if (posicionY + 24 > 550)
                {
                    CrearPagina();

                    // ENCABEZADO DE CONTINUACIÓN
                    gfx.DrawString(
                        "Detalle de movimientos (continuación)",
                        fuenteSeccion,
                        new XSolidBrush(ColorTexto),
                        35, posicionY);

                    posicionY += 12;

                    gfx.DrawRectangle(
                        new XSolidBrush(ColorPrincipal),
                        35, posicionY, anchoTotal, 25);

                    posicionX = 35;

                    for (int i = 0; i < encabezados.Length; i++)
                    {
                        gfx.DrawString(
                            encabezados[i],
                            fuenteNegrita,
                            XBrushes.White,
                            new XRect(
                                posicionX + 5,
                                posicionY,
                                anchosColumnas[i] - 10,
                                25),
                            XStringFormats.CenterLeft);

                        posicionX += anchosColumnas[i];
                    }

                    posicionY += 25;
                }

                XColor colorFila =
                    XColor.FromArgb(245, 248, 247);

                gfx.DrawRectangle(
                    new XSolidBrush(colorFila),
                    35, posicionY, anchoTotal, 24);

                string[] valores =
                {
                    movimiento.Tipo,
                    movimiento.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    movimiento.Concepto,
                    movimiento.MedioPago,
                    "L " + movimiento.Monto.ToString(
                        "N2",
                        CultureInfo.GetCultureInfo("es-HN"))
                };

                posicionX = 35;

                for (int i = 0; i < valores.Length; i++)
                {
                    string valor = AjustarTexto(
                        valores[i] ?? "",
                        fuenteNormal,
                        gfx,
                        anchosColumnas[i] - 12);

                    XBrush colorTexto = XBrushes.DarkSlateGray;

                    if (i == 0 &&
                        string.Equals(
                            movimiento.Tipo,
                            "INGRESO",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        colorTexto =
                            new XSolidBrush(ColorIngreso);
                    }
                    else if (i == 0 &&
                        string.Equals(
                            movimiento.Tipo,
                            "EGRESO",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        colorTexto =
                            new XSolidBrush(ColorEgreso);
                    }

                    XStringFormat formato = XStringFormats.CenterLeft;

                    gfx.DrawString(
                        valor,
                        fuenteNormal,
                        colorTexto,
                        new XRect(
                            posicionX + 6,
                            posicionY,
                            anchosColumnas[i] - 12,
                            24),
                        formato);

                    posicionX += anchosColumnas[i];
                }

                posicionY += 24;

                gfx.DrawLine(
                    new XPen(
                        XColor.FromArgb(220, 226, 224), 0.5),
                    35, posicionY,
                    35 + anchoTotal, posicionY);
            }

            // PIE DE PÁGINA
            gfx.Dispose();

            for (int i = 0; i < documento.Pages.Count; i++)
            {
                using XGraphics pie =
                    XGraphics.FromPdfPage(
                        documento.Pages[i],
                        XGraphicsPdfPageOptions.Append);

                pie.DrawLine(
                    new XPen(
                        XColor.FromArgb(210, 218, 216), 0.7),
                    35, 565, 806, 565);

                pie.DrawString(
                    "Assar Dental | Reporte financiero",
                    fuenteNormal,
                    XBrushes.Gray,
                    new XRect(35, 570, 400, 15),
                    XStringFormats.CenterLeft);

                pie.DrawString(
                    "Página " + (i + 1),
                    fuenteNormal,
                    XBrushes.Gray,
                    new XRect(700, 570, 105, 15),
                    XStringFormats.CenterRight);
            }

            documento.Save(rutaArchivo);
        }

        private static void DibujarTarjeta(
            XGraphics gfx,
            double x,
            double y,
            double ancho,
            double alto,
            string titulo,
            decimal monto,
            XColor color,
            XFont fuenteTitulo,
            XFont fuenteMonto)
        {
            gfx.DrawRectangle(
                new XSolidBrush(ColorFondo),
                x, y, ancho, alto);

            gfx.DrawString(
                titulo,
                fuenteTitulo,
                new XSolidBrush(color),
                new XRect(x + 10, y + 7, ancho - 20, 15),
                XStringFormats.CenterLeft);

            gfx.DrawString(
                "L " + monto.ToString(
                    "N2",
                    CultureInfo.GetCultureInfo("es-HN")),
                fuenteMonto,
                new XSolidBrush(color),
                new XRect(x + 10, y + 24, ancho - 20, 25),
                XStringFormats.CenterLeft);
        }

        private static string AjustarTexto(
            string texto,
            XFont fuente,
            XGraphics gfx,
            double anchoMaximo)
        {
            if (gfx.MeasureString(texto, fuente).Width <= anchoMaximo)
                return texto;

            while (texto.Length > 0 &&
                   gfx.MeasureString(texto + "...", fuente).Width >
                   anchoMaximo)
            {
                texto = texto.Substring(0, texto.Length - 1);
            }

            return texto + "...";
        }
    }
}
