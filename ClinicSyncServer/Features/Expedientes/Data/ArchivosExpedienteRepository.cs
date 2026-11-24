using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Expedientes.Projections;
using Dapper;
using System.Data;

namespace ClinicSyncServer.Features.Expedientes.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ArchivosExpedienteRepository(SqlConnectionFactory factory)
    {
        /// <summary>
        /// The factory
        /// </summary>
        private readonly SqlConnectionFactory _factory = factory;

        /// <summary>
        /// Insertars the asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="tipo">The tipo.</param>
        /// <param name="descripcion">The descripcion.</param>
        /// <param name="tamano">The tamano.</param>
        /// <param name="clave">The clave.</param>
        /// <param name="hash">The hash.</param>
        /// <param name="ct">The ct.</param>
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

        /// <summary>
        /// Obteners the por identifier asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="descripcion">The descripcion.</param>
        /// <param name="ct">The ct.</param>
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

        /// <summary>
        /// Cambiars the estado asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <param name="ct">The ct.</param>
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
