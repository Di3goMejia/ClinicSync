
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Expedientes.Requests;
using ClinicSyncServer.Features.Expedientes.Responses;
using Dapper;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Text.Json;

namespace ClinicSyncServer.Features.Expedientes.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class VisitasRepository(SqlConnectionFactory factory)
    {
        /// <summary>
        /// The factory
        /// </summary>
        private readonly SqlConnectionFactory _factory = factory;

        // LISTAR VISITAS
        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Listars the tratamientos asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
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
        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
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
        /// <summary>
        /// Insertars the completa asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Modificars the completa asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="request">The request.</param>
        /// <param name="ct">The ct.</param>
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
