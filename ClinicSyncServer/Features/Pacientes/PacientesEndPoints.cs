
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace ClinicSyncServer.Features.Pacientes
{
    public sealed class RegistrarPacienteRequest
    {
        public string Identidad { get; set; } = "";
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Genero { get; set; } = "";
        public string Telefono { get; set; } = "";
    }

    public sealed class ModificarPacienteRequest
    {
        public string Identidad { get; set; } = "";
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Genero { get; set; } = "";
        public string Telefono { get; set; } = "";
    }

    public sealed class CambiarEstadoPacienteRequest
    {
        public bool Activo { get; set; }
    }

    public static class PacientesEndpoints
    {
        // VALIDACIONES COMPARTIDAS
        private static bool DatosValidos(
            string identidad,
            string nombres,
            string apellidos,
            DateTime fechaNacimiento,
            string genero,
            string telefono)
        {
            return Regex.IsMatch(
                    identidad ?? "",
                    @"^\d{4}-\d{4}-\d{5}$")
                && !string.IsNullOrWhiteSpace(nombres)
                && nombres.Length <= 50
                && Regex.IsMatch(
                    nombres,
                    @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$")
                && !string.IsNullOrWhiteSpace(apellidos)
                && apellidos.Length <= 50
                && Regex.IsMatch(
                    apellidos,
                    @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$")
                && fechaNacimiento.Date >= new DateTime(1900, 1, 1)
                && fechaNacimiento.Date <= DateTime.Today
                && (genero == "M" || genero == "F")
                && Regex.IsMatch(
                    telefono ?? "",
                    @"^\d{4}-\d{4}$");
        }

        public static void MapPacientesEndpoints(
            this WebApplication app)
        {
            var grupo = app
                .MapGroup("/api/pacientes")
                .RequireAuthorization();

            // LISTAR PACIENTES
            grupo.MapGet("/", async (
                IPacientesService service,
                CancellationToken ct) =>
            {
                var pacientes = await service.ListarAsync(ct);

                return Results.Ok(pacientes);
            });

            // REGISTRAR PACIENTE
            grupo.MapPost("/", async (
                [FromBody] RegistrarPacienteRequest p,
                IPacientesService service,
                CancellationToken ct) =>
            {
                if (!DatosValidos(
                    p.Identidad,
                    p.Nombres,
                    p.Apellidos,
                    p.FechaNacimiento,
                    p.Genero,
                    p.Telefono))
                {
                    return Results.BadRequest(new
                    {
                        Mensaje = "Datos del paciente no válidos."
                    });
                }

                try
                {
                    await service.InsertarAsync(p, ct);

                    return Results.StatusCode(201);
                }
                catch (SqlException ex)
                    when (ex.Number == 2601 ||
                          ex.Number == 2627)
                {
                    return Results.Conflict(new
                    {
                        Mensaje =
                            "Ya existe un paciente con esta identidad."
                    });
                }
            });

            // EDITAR PACIENTE
            grupo.MapPut("/{idPaciente:int}", async (
                int idPaciente,
                [FromBody] ModificarPacienteRequest p,
                IPacientesService service,
                CancellationToken ct) =>
            {
                if (idPaciente <= 0)
                {
                    return Results.BadRequest(new
                    {
                        Mensaje = "El ID del paciente no es válido."
                    });
                }

                if (!DatosValidos(
                    p.Identidad,
                    p.Nombres,
                    p.Apellidos,
                    p.FechaNacimiento,
                    p.Genero,
                    p.Telefono))
                {
                    return Results.BadRequest(new
                    {
                        Mensaje = "Datos del paciente no válidos."
                    });
                }

                try
                {
                    await service.ModificarAsync(
                        idPaciente, p, ct);

                    return Results.NoContent();
                }
                catch (SqlException ex)
                    when (ex.Number == 2601 ||
                          ex.Number == 2627)
                {
                    return Results.Conflict(new
                    {
                        Mensaje =
                            "Ya existe un paciente con esta identidad."
                    });
                }
                catch (SqlException ex)
                    when (ex.Number == 50006)
                {
                    return Results.Conflict(new
                    {
                        Mensaje =
                            "El paciente no existe o está inactivo."
                    });
                }
            });

            // ACTIVAR O DESACTIVAR PACIENTE
            grupo.MapPatch("/{idPaciente:int}/estado",
                async (
                    int idPaciente,
                    [FromBody] CambiarEstadoPacienteRequest p,
                    IPacientesService service,
                    CancellationToken ct) =>
                {
                    try
                    {
                        await service.CambiarEstadoAsync(
                            idPaciente,
                            p.Activo,
                            ct);

                        return Results.NoContent();
                    }
                    catch (SqlException ex)
                        when (ex.Number == 50001)
                    {
                        return Results.NotFound(new
                        {
                            Mensaje = "El paciente no existe."
                        });
                    }
                });
        }
    }
}
