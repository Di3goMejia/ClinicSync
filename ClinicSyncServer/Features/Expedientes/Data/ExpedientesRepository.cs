
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Expedientes.Responses;
using Dapper;
using System.Data;

namespace ClinicSyncServer.Features.Expedientes.Data
{
    public class ExpedientesRepository(SqlConnectionFactory factory)
    {
        private readonly SqlConnectionFactory _factory = factory;

        // OBTENER EXPEDIENTE
        public async Task<ExpedienteDetalle?> ObtenerPorIdAsync(
            int? idExpediente = null, int? idPaciente = null,
            CancellationToken ct = default)
        {
            if (idExpediente is null && idPaciente is null)
                throw new InvalidOperationException("Debe especificar un ID de expediente o un ID de paciente.");

            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_Expedientes_ObtenerPorId",
                new { IdExpediente = idExpediente, IdPaciente = idPaciente },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QuerySingleAsync<ExpedienteDetalle>(comando);
        }

        // MODIFICAR EXPEDIENTE
        public async Task ModificarAsync(int idPaciente, string? notas = null,
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_Expedientes_Modificar",
                new { IdPaciente = idPaciente, Notas = notas },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);
        }
    }
}
