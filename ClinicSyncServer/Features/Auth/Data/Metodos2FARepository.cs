using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Infrastructure.Facial;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ClinicSyncServer.Features.Auth.Data
{
    public class Metodos2FARepository(
        SqlConnectionFactory factory,
        ProtectorCredencialFacial protector)
    {
        public async Task<byte[]?> ObtenerCredencialActivaAsync(
            int idUsuario,
            CancellationToken ct = default)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync(ct);

            byte[]? credencialProtegida =
                await db.QuerySingleOrDefaultAsync<byte[]>(
                    new CommandDefinition(
                        """
                        SELECT TOP (1) Credencial
                        FROM Metodos2FA
                        WHERE IdUsuario = @IdUsuario
                          AND Tipo = 'FACIAL'
                          AND Activo = 1
                        ORDER BY FechaCreacion DESC, IdMetodo DESC;
                        """,
                        new { IdUsuario = idUsuario },
                        cancellationToken: ct));

            if (credencialProtegida == null)
                return null;

            return protector.Desproteger(credencialProtegida);
        }

        public async Task<bool> TieneMetodoActivoAsync(
            int idUsuario,
            CancellationToken ct = default)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync(ct);

            return await db.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                    """
                    SELECT CAST(
                        CASE WHEN EXISTS (
                            SELECT 1
                            FROM Metodos2FA
                            WHERE IdUsuario = @IdUsuario
                              AND Tipo = 'FACIAL'
                              AND Activo = 1
                        )
                        THEN 1 ELSE 0 END
                    AS BIT);
                    """,
                    new { IdUsuario = idUsuario },
                    cancellationToken: ct));
        }

        public async Task<int> RegistrarAsync(
            int idUsuario,
            byte[] credencial,
            CancellationToken ct = default)
        {
            if (credencial == null || credencial.Length == 0)
            {
                throw new ArgumentException(
                    "La credencial facial no puede estar vacía.",
                    nameof(credencial));
            }

            // Cifrar antes de almacenar en SQL Server.
            byte[] credencialProtegida = protector.Proteger(credencial);

            await using SqlConnection db = factory.Crear();
            await db.OpenAsync(ct);

            const string sql = """
                SET XACT_ABORT ON;

                BEGIN TRANSACTION;

                UPDATE Metodos2FA
                SET Activo = 0
                WHERE IdUsuario = @IdUsuario
                  AND Tipo = 'FACIAL'
                  AND Activo = 1;

                INSERT INTO Metodos2FA
                (
                    IdUsuario,
                    Tipo,
                    Credencial,
                    Activo
                )
                VALUES
                (
                    @IdUsuario,
                    'FACIAL',
                    @Credencial,
                    1
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);

                COMMIT TRANSACTION;
                """;

            return await db.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        IdUsuario = idUsuario,
                        Credencial = credencialProtegida
                    },
                    cancellationToken: ct));
        }

        public async Task<bool> DesactivarAsync(
            int idUsuario,
            CancellationToken ct = default)
        {
            await using SqlConnection db = factory.Crear();
            await db.OpenAsync(ct);

            int filas = await db.ExecuteAsync(
                new CommandDefinition(
                    """
                    UPDATE Metodos2FA
                    SET Activo = 0
                    WHERE IdUsuario = @IdUsuario
                      AND Tipo = 'FACIAL'
                      AND Activo = 1;
                    """,
                    new { IdUsuario = idUsuario },
                    cancellationToken: ct));

            return filas > 0;
        }
    }
}
