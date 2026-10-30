/* ============================================================
   MODULO FINANZAS - EGRESOS
   Servicios: registra el gasto sin tocar el inventario.
   Materiales: crea la compra, su detalle y la entrada de stock
   usando api.usp_MovimientosInventario_Insertar, todo en una
   sola transaccion.
   Requiere: Usuarios, Compras, DetalleCompras, Materiales,
   MovimientosInventario y api.usp_MovimientosInventario_Insertar.
   ============================================================ */

USE clinicsync;
GO

CREATE TABLE dbo.Egresos(
    IdEgreso   INT IDENTITY NOT NULL
        CONSTRAINT PK_Egresos PRIMARY KEY,

    IdUsuario  INT NOT NULL
        CONSTRAINT FK_Egresos_Usuarios
            FOREIGN KEY REFERENCES dbo.Usuarios(IdUsuario),

    IdCompra   INT NULL
        CONSTRAINT FK_Egresos_Compras
            FOREIGN KEY REFERENCES dbo.Compras(IdCompra),

    Tipo       VARCHAR(20) NOT NULL
        CONSTRAINT CK_Egresos_Tipo
            CHECK (Tipo IN ('MATERIALES', 'SERVICIOS')),

    Fecha      DATE NOT NULL,

    Concepto   VARCHAR(255) NOT NULL,

    MedioPago  VARCHAR(20) NOT NULL
        CONSTRAINT CK_Egresos_MedioPago
            CHECK (MedioPago IN ('EFECTIVO', 'TRANSFERENCIA')),

    Monto      DECIMAL(10,2) NOT NULL
        CONSTRAINT CK_Egresos_Monto CHECK (Monto > 0),

    Activo     BIT NOT NULL
        CONSTRAINT DF_Egresos_Activo DEFAULT 1,

    CONSTRAINT CK_Egresos_Compra CHECK (
        (Tipo = 'MATERIALES' AND IdCompra IS NOT NULL)
        OR (Tipo = 'SERVICIOS' AND IdCompra IS NULL)
    )
);
GO

-- EGRESOS
CREATE OR ALTER PROCEDURE api.usp_Egresos_Insertar
    @IdUsuario  INT,
    @Tipo       VARCHAR(20),
    @Fecha      DATE,
    @Concepto   VARCHAR(255) = NULL,
    @MedioPago  VARCHAR(20),
    @Monto      DECIMAL(10,2),
    @Materiales NVARCHAR(MAX) = NULL,
    @IdEgreso   INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Tipo      = UPPER(TRIM(@Tipo));
    SET @Concepto  = NULLIF(TRIM(@Concepto), '');
    SET @MedioPago = UPPER(TRIM(@MedioPago));

    IF @Tipo IS NULL OR @Tipo NOT IN ('SERVICIOS', 'MATERIALES')
        THROW 50001, 'El tipo de egreso no es válido.', 1;

    IF @Fecha IS NULL
        THROW 50002, 'La fecha es obligatoria.', 1;

    IF @Tipo = 'SERVICIOS' AND @Concepto IS NULL
        THROW 50003, 'El concepto es obligatorio.', 1;

    IF @MedioPago IS NULL OR @MedioPago NOT IN ('EFECTIVO', 'TRANSFERENCIA')
        THROW 50004, 'El medio de pago no es válido.', 1;

    IF @Monto IS NULL OR @Monto <= 0
        THROW 50005, 'El monto debe ser mayor que cero.', 1;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.Usuarios
        WHERE IdUsuario = @IdUsuario AND Estado = 'ACTIVO'
    )
        THROW 50006, 'El usuario no existe o está inactivo.', 1;

    DECLARE @Items    TABLE (IdMaterial INT NULL, Cantidad DECIMAL(12,2) NULL);
    DECLARE @Resumen  TABLE (IdMaterial INT NOT NULL, Cantidad DECIMAL(12,2) NOT NULL);
    DECLARE @Detalles TABLE (IdDetalleCompra INT NOT NULL,
                             IdMaterial INT NOT NULL,
                             Cantidad DECIMAL(12,2) NOT NULL);
    DECLARE @IdCompra INT = NULL;

    IF @Tipo = 'MATERIALES'
    BEGIN
        IF @Materiales IS NULL OR ISJSON(@Materiales) <> 1
            THROW 50007, 'Debe seleccionar al menos un material.', 1;

        INSERT INTO @Items (IdMaterial, Cantidad)
        SELECT IdMaterial, Cantidad
        FROM OPENJSON(@Materiales)
             WITH (IdMaterial INT '$.IdMaterial',
                   Cantidad   DECIMAL(12,2) '$.Cantidad');

        IF NOT EXISTS (SELECT 1 FROM @Items)
            THROW 50007, 'Debe seleccionar al menos un material.', 1;

        IF EXISTS (SELECT 1 FROM @Items
                   WHERE IdMaterial IS NULL OR Cantidad IS NULL OR Cantidad <= 0)
            THROW 50008, 'La cantidad de cada material debe ser mayor que cero.', 1;

        IF EXISTS (
            SELECT 1
            FROM @Items i
            LEFT JOIN dbo.Materiales m
                   ON m.IdMaterial = i.IdMaterial AND m.Activo = 1
            WHERE m.IdMaterial IS NULL
        )
            THROW 50009, 'Alguno de los materiales no existe o está inactivo.', 1;

        INSERT INTO @Resumen (IdMaterial, Cantidad)
        SELECT IdMaterial, SUM(Cantidad)
        FROM @Items
        GROUP BY IdMaterial;

        -- El concepto se arma con los materiales elegidos.
        SELECT @Concepto = LEFT('Compra: ' + STRING_AGG(
                   m.Nombre + ' (' +
                   CASE WHEN r.Cantidad = FLOOR(r.Cantidad)
                        THEN CONVERT(VARCHAR(20), CAST(r.Cantidad AS BIGINT))
                        ELSE CONVERT(VARCHAR(20), r.Cantidad)
                   END + ')', ', '), 255)
        FROM @Resumen r
        JOIN dbo.Materiales m ON m.IdMaterial = r.IdMaterial;
    END;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Tipo = 'MATERIALES'
        BEGIN
            INSERT INTO dbo.Compras (Fecha)
            VALUES (CAST(@Fecha AS DATETIME2));

            SET @IdCompra = CAST(SCOPE_IDENTITY() AS INT);

            INSERT INTO dbo.DetalleCompras
                (IdCompra, IdMaterial, Cantidad, PrecioUnitario)
            OUTPUT inserted.IdDetalleCompra, inserted.IdMaterial, inserted.Cantidad
                INTO @Detalles (IdDetalleCompra, IdMaterial, Cantidad)
            SELECT @IdCompra, r.IdMaterial, r.Cantidad, m.PrecioCompra
            FROM @Resumen r
            JOIN dbo.Materiales m ON m.IdMaterial = r.IdMaterial;

            DECLARE @IdDetalle INT, @IdMat INT, @Cant DECIMAL(12,2);

            DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
                SELECT IdDetalleCompra, IdMaterial, Cantidad FROM @Detalles;

            OPEN cur;
            FETCH NEXT FROM cur INTO @IdDetalle, @IdMat, @Cant;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                EXEC api.usp_MovimientosInventario_Insertar
                    @IdMaterial      = @IdMat,
                    @IdUsuario       = @IdUsuario,
                    @Tipo            = 'ENTRADA',
                    @Cantidad        = @Cant,
                    @IdDetalleCompra = @IdDetalle;

                FETCH NEXT FROM cur INTO @IdDetalle, @IdMat, @Cant;
            END;

            CLOSE cur;
            DEALLOCATE cur;
        END;

        INSERT INTO dbo.Egresos
            (IdUsuario, IdCompra, Tipo, Fecha, Concepto, MedioPago, Monto)
        VALUES
            (@IdUsuario, @IdCompra, @Tipo, @Fecha, @Concepto, @MedioPago, @Monto);

        SET @IdEgreso = CAST(SCOPE_IDENTITY() AS INT);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE api.usp_Egresos_Listar
    @FechaDesde DATE,
    @FechaHasta DATE,
    @Activo     BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @FechaDesde IS NULL OR @FechaHasta IS NULL
        THROW 50001, 'El rango de fechas es obligatorio.', 1;

    IF @FechaDesde > @FechaHasta
        THROW 50002, 'La fecha inicial no puede ser mayor que la final.', 1;

    SELECT   IdEgreso, Tipo, Fecha, Concepto, MedioPago, Monto, Activo, IdCompra
    FROM     dbo.Egresos
    WHERE    Fecha BETWEEN @FechaDesde AND @FechaHasta AND
             (@Activo IS NULL OR Activo = @Activo)
    ORDER BY Fecha DESC, IdEgreso DESC;
END;
GO
