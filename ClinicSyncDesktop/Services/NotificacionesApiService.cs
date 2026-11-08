
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Services;

public sealed class NotificacionesApiService
{
    // CONSULTAR NOTIFICACIONES
    public async Task<Notificacion[]> ListarAsync(
        string? tipo = null,
        string? estado = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        string? buscar = null)
    {
        var valores = new Dictionary<string, string?>
        {
            ["tipo"] = tipo,
            ["estado"] = estado,
            ["desde"] = desde?.ToString("yyyy-MM-dd"),
            ["hasta"] = hasta?.ToString("yyyy-MM-dd"),
            ["buscar"] = buscar
        };

        var parametros = string.Join("&",
            valores
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x =>
                    Uri.EscapeDataString(x.Key) + "=" +
                    Uri.EscapeDataString(x.Value!)));

        var ruta = "api/notificaciones/" +
            (parametros.Length == 0
                ? ""
                : "?" + parametros);

        return await ClienteApi
            .ObtenerAutenticadoAsync<Notificacion[]>(ruta)
            ?? Array.Empty<Notificacion>();
    }

    // MARCAR UNA COMO LEÍDA
    public async Task MarcarLeidaAsync(long id)
    {
        using var respuesta =
            await ClienteApi.PatchAutenticadoAsync(
                $"api/notificaciones/{id}/leida",
                new { });

        respuesta.EnsureSuccessStatusCode();
    }

    // MARCAR TODAS COMO LEÍDAS
    public async Task MarcarTodasAsync()
    {
        using var respuesta =
            await ClienteApi.PatchAutenticadoAsync(
                "api/notificaciones/leer-todas",
                new { });

        respuesta.EnsureSuccessStatusCode();
    }

    // ARCHIVAR
    public async Task ArchivarAsync(long id)
    {
        using var respuesta =
            await ClienteApi.PatchAutenticadoAsync(
                $"api/notificaciones/{id}/archivar",
                new { });

        respuesta.EnsureSuccessStatusCode();
    }
}

