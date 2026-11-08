
using System;

namespace ClinicSyncDesktop.Models;

public sealed class Notificacion
{
    public long IdNotificacion { get; set; }

    public string Tipo { get; set; } = "";

    public int IdReferencia { get; set; }

    public string Titulo { get; set; } = "";

    public string Descripcion { get; set; } = "";

    public DateTime FechaEvento { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Leida { get; set; }

    public bool Vigente { get; set; }

    public bool Archivada { get; set; }

    public string Icono =>
        Tipo == "CITA" ? "▣" : "♧";

    public string FechaTexto =>
        FechaCreacion.ToString("dd/MM/yyyy, hh:mm tt");
}

