
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Expedientes.Requests;
using ClinicSyncServer.Features.Expedientes.Responses;
using Dapper;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Text.Json;

namespace ClinicSyncServer.Features.Expedientes.Data
{
    public class VisitasRepository(SqlConnectionFactory factory)
    {
        private readonly SqlConnectionFactory _factory = factory;

        // LISTAR VISITAS
        public async Task<IEnumerable<VisitaListado>> ListarAsync(
            int idPaciente,
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_Visitas_Listar",
                new { IdPaciente = idPaciente },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QueryAsync<VisitaListado>(comando);
        }

        // OBTENER VISITA
        public async Task<VisitaListado?> ObtenerPorIdAsync(
            int idVisita,
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_Visitas_ObtenerPorId",
                new { IdVisita = idVisita },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QueryFirstOrDefaultAsync<VisitaListado>(
                comando);
        }

        // LISTAR TRATAMIENTOS DE UNA VISITA
        public async Task<IEnumerable<TratamientoVisitaListado>>
            ListarTratamientosAsync(
                int idVisita,
                CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_TratamientosVisita_Listar",
                new { IdVisita = idVisita },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QueryAsync<TratamientoVisitaListado>(comando);
        }

        // INSERTAR VISITA
        public async Task<int> InsertarAsync(
            CrearVisitaRequest request,
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_Visitas_Insertar",
                new
                {
                    request.IdPaciente,
                    request.IdEmpleado,
                    request.Fecha,
                    request.Motivo,
                    request.Notas
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QuerySingleAsync<int>(comando);
        }

        // MODIFICAR VISITA
        public async Task ModificarAsync(
            int idVisita,
            ModificarVisitaRequest request,
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_Visitas_Modificar",
                new
                {
                    IdVisita = idVisita,
                    request.IdEmpleado,
                    request.Fecha,
                    request.Motivo,
                    request.Notas
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);
        }

        // CAMBIAR ESTADO DE VISITA
        public async Task CambiarEstadoAsync(int idVisita, bool activo, CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_Visitas_CambiarEstado",
                new { IdVisita = idVisita, Activo = activo },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);
        }

        // REGISTRAR VISITA CON SUS TRATAMIENTOS
        public async Task<int> InsertarCompletaAsync(
            CrearVisitaCompletaRequest request,
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            string tratamientosJson =
                JsonSerializer.Serialize(request.Tratamientos);

            var comando = new CommandDefinition(
                "api.usp_Visitas_CrearCompleta",
                new
                {
                    request.IdPaciente,
                    request.IdEmpleado,
                    request.Fecha,
                    request.Motivo,
                    request.Notas,
                    TratamientosJson = tratamientosJson
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QuerySingleAsync<int>(comando);
        }

        // MODIFICAR VISITA CON SUS TRATAMIENTOS
        public async Task ModificarCompletaAsync(
            int idVisita,
            ModificarVisitaCompletaRequest request,
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            string tratamientosJson =
                JsonSerializer.Serialize(request.Tratamientos);

            var comando = new CommandDefinition(
                "api.usp_Visitas_ModificarCompleta",
                new
                {
                    IdVisita = idVisita,
                    request.IdEmpleado,
                    request.Fecha,
                    request.Motivo,
                    request.Notas,
                    TratamientosJson = tratamientosJson
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);
        }
    }
}
