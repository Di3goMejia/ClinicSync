using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Expedientes.Projections;
using Dapper;
using System.Data;

namespace ClinicSyncServer.Features.Expedientes.Data
{
    public class ArchivosExpedienteRepository(SqlConnectionFactory factory)
    {
        private readonly SqlConnectionFactory _factory = factory;
        
        public async Task InsertarAsync(Guid idArchivo, int idPaciente, 
            int? idVisita, int idUsuario, string nombre, string tipo, 
            string? descripcion, long tamano, string clave, string hash,
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_ArchivosExpediente_Insertar",
                new
                {
                    IdArchivo = idArchivo,
                    IdPaciente = idPaciente,
                    IdVisita = idVisita,
                    IdUsuario  = idUsuario,
                    NombreArchivo = nombre,
                    ClaveAlmacen = clave,
                    Descripcion = descripcion,
                    HashArchivo = hash,
                    TipoMime = tipo,
                    TamanoBytes = tamano
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);
        }

        public async Task<ArchivoExpediente?> ObtenerPorIdAsync(Guid idArchivo, CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_ArchivosExpediente_ObtenerPorId",
                new { IdArchivo = idArchivo },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QuerySingleAsync<ArchivoExpediente>(comando);
        }

        public async Task<IEnumerable<ArchivoExpediente>> ListarAsync(
            int idPaciente, int? idVisita = null, CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_ArchivosExpediente_Listar",
                new { IdPaciente = idPaciente, IdVisita = idVisita },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QueryAsync<ArchivoExpediente>(comando);
        }

        public async Task ModificarAsync(Guid idArchivo, string nombre, 
            string? descripcion = null, CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_ArchivosExpediente_Modificar",
                new { IdArchivo = idArchivo, NombreArchivo = nombre, Descripcion = descripcion },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);

        }

        public async Task CambiarEstadoAsync(Guid idArchivo, bool activo, 
            CancellationToken ct = default)
        {
            await using var db = _factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                "api.usp_ArchivosExpediente_CambiarEstado",
                new { IdArchivo = idArchivo, Activo = activo },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);
        }
    }
}
