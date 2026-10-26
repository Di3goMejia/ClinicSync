
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Expedientes.Requests;
using ClinicSyncServer.Features.Expedientes.Responses;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSyncServer.Features.Expedientes.Data
{
    public class ExpedientesRepository
    {
        private readonly SqlConnectionFactory _factory;

        public ExpedientesRepository(SqlConnectionFactory factory)
        {
            _factory = factory;
        }

        // OBTENER EXPEDIENTE
        public async Task<ExpedienteDetalle?> ObtenerExpedienteAsync(
            int idPaciente,
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_Expedientes_ObtenerPorId",
                new { IdPaciente = idPaciente },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QueryFirstOrDefaultAsync<ExpedienteDetalle>(
                comando);
        }

        // LISTAR VISITAS
        public async Task<IEnumerable<VisitaListado>> ListarVisitasAsync(
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
        public async Task<VisitaListado?> ObtenerVisitaAsync(
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

            return await db.QueryAsync<TratamientoVisitaListado>(
                comando);
        }

        // REGISTRAR VISITA
        public async Task<int> CrearVisitaAsync(
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
        public async Task ModificarVisitaAsync(
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

        // REGISTRAR VISITA CON SUS TRATAMIENTOS
        public async Task<int> CrearVisitaCompletaAsync(
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
        public async Task ModificarVisitaCompletaAsync(
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
