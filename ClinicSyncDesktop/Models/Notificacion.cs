
using System;

namespace ClinicSyncDesktop.Models;

/// <summary>
/// 
/// </summary>
public sealed class Notificacion
{
    /// <summary>
    /// Gets or sets the identifier notificacion.
    /// </summary>
    /// <value>
    /// The identifier notificacion.
    /// </value>
    public long IdNotificacion { get; set; }

    /// <summary>
    /// Gets or sets the tipo.
    /// </summary>
    /// <value>
    /// The tipo.
    /// </value>
    public string Tipo { get; set; } = "";

    /// <summary>
    /// Gets or sets the identifier referencia.
    /// </summary>
    /// <value>
    /// The identifier referencia.
    /// </value>
    public int IdReferencia { get; set; }

    /// <summary>
    /// Gets or sets the titulo.
    /// </summary>
    /// <value>
    /// The titulo.
    /// </value>
    public string Titulo { get; set; } = "";

    /// <summary>
    /// Gets or sets the descripcion.
    /// </summary>
    /// <value>
    /// The descripcion.
    /// </value>
    public string Descripcion { get; set; } = "";

    /// <summary>
    /// Gets or sets the fecha evento.
    /// </summary>
    /// <value>
    /// The fecha evento.
    /// </value>
    public DateTime FechaEvento { get; set; }

    /// <summary>
    /// Gets or sets the fecha creacion.
    /// </summary>
    /// <value>
    /// The fecha creacion.
    /// </value>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Notificacion"/> is leida.
    /// </summary>
    /// <value>
    ///   <c>true</c> if leida; otherwise, <c>false</c>.
    /// </value>
    public bool Leida { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Notificacion"/> is vigente.
    /// </summary>
    /// <value>
    ///   <c>true</c> if vigente; otherwise, <c>false</c>.
    /// </value>
    public bool Vigente { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="Notificacion"/> is archivada.
    /// </summary>
    /// <value>
    ///   <c>true</c> if archivada; otherwise, <c>false</c>.
    /// </value>
    public bool Archivada { get; set; }

    /// <summary>
    /// Gets the icono.
    /// </summary>
    /// <value>
    /// The icono.
    /// </value>
    public string Icono =>
        Tipo == "CITA" ? "▣" : "♧";

    /// <summary>
    /// Gets the fecha texto.
    /// </summary>
    /// <value>
    /// The fecha texto.
    /// </value>
    public string FechaTexto =>
        FechaCreacion.ToString("dd/MM/yyyy, hh:mm tt");
}

