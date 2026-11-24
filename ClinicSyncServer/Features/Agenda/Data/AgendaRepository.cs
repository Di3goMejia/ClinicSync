using ClinicSyncServer.Common;
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Agenda.Requests;
using ClinicSyncServer.Features.Agenda.Responses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicSyncServer.Features.Agenda.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class AgendaRepository(SqlConnectionFactory factory)
    {
        // Convierte el error de SQL en una excepción propia.
        // 50009 (horario ocupado) y 50010 (paciente ocupado) son conflictos: HTTP 409.
        // Los demás errores de Agenda siguen el camino normal (HTTP 400).
        /// <summary>
        /// Traducirs the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        private static Exception Traducir(SqlException ex) =>
            ex.Number is 50009 or 50010
                ? new ConflictoException(ex.Message)
                : SqlErrorTranslator.Traducir(ex);

        // Parámetros comunes de registrar y modificar una cita.
        /// <summary>
        /// Parametroses the cita.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <returns></returns>
        private static DynamicParameters ParametrosCita(CitaRequest req)
        {
            var p = new DynamicParameters();

            p.Add("@IdPaciente", req.IdPaciente!.Value, DbType.Int32);
            p.Add("@FechaInicio", req.FechaInicio!.Value, DbType.DateTime2);
            p.Add("@FechaFinal", req.FechaFinal!.Value, DbType.DateTime2);
            p.Add("@Titulo", req.Titulo, DbType.String, size: 100);
            p.Add("@Notas", req.Notas, DbType.String, size: 500);
            p.Add("@Color", req.Color, DbType.String, size: 7);
            p.Add("@Direccion", req.Direccion, DbType.String, size: 200);
            p.Add("@TodoElDia", req.TodoElDia, DbType.Boolean);

            return p;
        }

        // Convierte el usuario con sesión (sub del token) en su IdEmpleado.
        /// <summary>
        /// Obteners the identifier empleado asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<int?> ObtenerIdEmpleadoAsync(
            int idUsuario,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            var cmd = new CommandDefinition(
                "api.usp_Agenda_ObtenerIdEmpleado",
                new { IdUsuario = idUsuario },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QuerySingleOrDefaultAsync<int?>(cmd);
        }

        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="idEmpleado">The identifier empleado.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<int> InsertarAsync(
            CitaRequest req,
            int idEmpleado,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var p = ParametrosCita(req);
                p.Add("@IdEmpleado", idEmpleado, DbType.Int32);

                var cmd = new CommandDefinition(
                    "api.usp_Citas_Insertar",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                return await db.QuerySingleAsync<int>(cmd);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw Traducir(ex);
            }
        }



        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<IEnumerable<EventoAgendaResponse>> ListarAsync(
            ListarAgendaQuery query,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var p = new DynamicParameters();
                p.Add("@Desde", query.Desde!.Value, DbType.DateTime2);
                p.Add("@Hasta", query.Hasta!.Value, DbType.DateTime2);
                p.Add("@IdEmpleado", query.IdEmpleado, DbType.Int32);
                p.Add("@IdPaciente", query.IdPaciente, DbType.Int32);

                var cmd = new CommandDefinition(
                    "api.usp_Agenda_Listar",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                return await db.QueryAsync<EventoAgendaResponse>(cmd);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw Traducir(ex);
            }
        }

        /// <summary>
        /// Proximases the asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<IEnumerable<ProximaCitaResponse>> ProximasAsync(
            ProximasCitasQuery query,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var p = new DynamicParameters();
                p.Add("@Desde", query.Desde!.Value, DbType.DateTime2);
                p.Add("@Cantidad", query.Cantidad, DbType.Int32);
                p.Add("@IdEmpleado", query.IdEmpleado, DbType.Int32);

                var cmd = new CommandDefinition(
                    "api.usp_Agenda_Proximas",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                return await db.QueryAsync<ProximaCitaResponse>(cmd);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw Traducir(ex);
            }
        }

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<EventoAgendaResponse?> ObtenerPorIdAsync(
            int idEvento,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var cmd = new CommandDefinition(
                    "api.usp_Agenda_ObtenerPorId",
                    new { IdEvento = idEvento },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                return await db.QuerySingleOrDefaultAsync<EventoAgendaResponse>(cmd);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw Traducir(ex);
            }
        }

        /// <summary>
        /// Verificars the disponibilidad asynchronous.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="idEmpleado">The identifier empleado.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<DisponibilidadResponse> VerificarDisponibilidadAsync(
            DisponibilidadQuery query,
            int idEmpleado,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var p = new DynamicParameters();
                p.Add("@IdEmpleado", idEmpleado, DbType.Int32);
                p.Add("@FechaInicio", query.FechaInicio!.Value, DbType.DateTime2);
                p.Add("@FechaFinal", query.FechaFinal!.Value, DbType.DateTime2);
                p.Add("@IdEventoExcluir", query.IdEventoExcluir, DbType.Int32);

                var cmd = new CommandDefinition(
                    "api.usp_Agenda_VerificarDisponibilidad",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                var conflictos = (await db.QueryAsync<ConflictoAgendaResponse>(cmd)).ToList();

                // 0 filas = el horario está libre.
                return new DisponibilidadResponse
                {
                    Disponible = conflictos.Count == 0,
                    Conflictos = conflictos
                };
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw Traducir(ex);
            }
        }

        // MODIFICAR UNA CITA
        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        public async Task ModificarAsync(
            int idEvento,
            CitaRequest req,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var p = ParametrosCita(req);

                p.Add("@IdEvento", idEvento, DbType.Int32);

                var cmd = new CommandDefinition(
                    "api.usp_Citas_Modificar",
                    p,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                await db.ExecuteAsync(cmd);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw Traducir(ex);
            }
        }

        // CANCELAR UNA CITA
        /// <summary>
        /// Cancelars the asynchronous.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        /// <param name="ct">The ct.</param>
        public async Task CancelarAsync(
            int idEvento,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            try
            {
                var cmd = new CommandDefinition(
                    "api.usp_Citas_Cancelar",
                    new { IdEvento = idEvento },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: ct);

                await db.ExecuteAsync(cmd);
            }
            catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
            {
                throw Traducir(ex);
            }
        }
    }
}