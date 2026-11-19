/* Procedimientos para la relación Roles - PermisosRol. */

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'api')
	EXEC(N'CREATE SCHEMA api');
GO

CREATE OR ALTER PROCEDURE api.usp_Roles_PermisosPorRol
	@IdRol INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT p.Codigo
	FROM dbo.PermisosRol AS pr
	INNER JOIN dbo.Permisos AS p ON p.IdPermiso = pr.IdPermiso
	WHERE pr.IdRol = @IdRol
	ORDER BY p.Codigo;
END;
GO

CREATE OR ALTER PROCEDURE api.usp_Roles_ReemplazarPermisos
	@IdRol INT,
	@CodigosPermisos dbo.CodigosPermiso READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	BEGIN TRANSACTION;

	DELETE FROM dbo.PermisosRol
	WHERE IdRol = @IdRol;

	INSERT INTO dbo.PermisosRol (IdRol, IdPermiso)
	SELECT @IdRol, p.IdPermiso
	FROM dbo.Permisos AS p
	INNER JOIN @CodigosPermisos AS c ON c.Codigo = p.Codigo;

	COMMIT TRANSACTION;
END;
GO
