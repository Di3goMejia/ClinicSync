
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Pacientes.Responses;
using Dapper;
using System.Data;

namespace ClinicSyncServer.Features.Pacientes.Data
{
    public sealed class PacientesRepository
    {
        private readonly SqlConnectionFactory factory;

        public PacientesRepository(SqlConnectionFactory factory)
        {
            this.factory = factory;
        }

        // LISTAR PACIENTES
        public async Task<IEnumerable<PacienteListado>> ListarAsync(
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            var comando = new CommandDefinition(
                commandText: "api.usp_Pacientes_Listar",
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            return await db.QueryAsync<PacienteListado>(comando);
        }

        // REGISTRAR PACIENTE
        public async Task InsertarAsync(
            RegistrarPacienteRequest paciente,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            var parametros = CrearParametrosPaciente(
                paciente.Identidad,
                paciente.Nombres,
                paciente.Apellidos,
                paciente.FechaNacimiento,
                paciente.Genero,
                paciente.Telefono);

            var comando = new CommandDefinition(
                commandText: "api.usp_Pacientes_Insertar",
                parameters: parametros,
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);
        }

        // MODIFICAR DATOS DEL PACIENTE
        public async Task ModificarAsync(
            int idPaciente,
            ModificarPacienteRequest paciente,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            var parametros = CrearParametrosPaciente(
                paciente.Identidad,
                paciente.Nombres,
                paciente.Apellidos,
                paciente.FechaNacimiento,
                paciente.Genero,
                paciente.Telefono);

            parametros.Add(
                "@IdPaciente",
                idPaciente,
                DbType.Int32);

            var comando = new CommandDefinition(
                commandText: "api.usp_Pacientes_Modificar",
                parameters: parametros,
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);
        }

        // CAMBIAR ESTADO DEL PACIENTE
        public async Task CambiarEstadoAsync(
            int idPaciente,
            bool activo,
            CancellationToken ct = default)
        {
            await using var db = factory.Crear();
            await db.OpenAsync(ct);

            var parametros = new DynamicParameters();

            parametros.Add(
                "@IdPaciente",
                idPaciente,
                DbType.Int32);

            parametros.Add(
                "@Activo",
                activo,
                DbType.Boolean);

            var comando = new CommandDefinition(
                commandText: "api.usp_Pacientes_CambiarEstado",
                parameters: parametros,
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct);

            await db.ExecuteAsync(comando);
        }

        // PARÁMETROS COMPARTIDOS PARA REGISTRAR Y EDITAR
        private static DynamicParameters CrearParametrosPaciente(
            string identidad,
            string nombres,
            string apellidos,
            DateTime fechaNacimiento,
            string genero,
            string telefono)
        {
            var parametros = new DynamicParameters();

            parametros.Add(
                "@Identidad",
                identidad,
                DbType.AnsiString,
                size: 20);

            parametros.Add(
                "@Nombres",
                nombres,
                DbType.AnsiString,
                size: 50);

            parametros.Add(
                "@Apellidos",
                apellidos,
                DbType.AnsiString,
                size: 50);

            parametros.Add(
                "@FechaNacimiento",
                fechaNacimiento,
                DbType.Date);

            parametros.Add(
                "@Genero",
                genero,
                DbType.AnsiStringFixedLength,
                size: 1);

            parametros.Add(
                "@Telefono",
                telefono,
                DbType.AnsiString,
                size: 20);

            return parametros;
        }
    }
}
