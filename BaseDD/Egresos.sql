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
CREATE PROCEDURE api.usp_Egresos_Insertar
    @IdUsuario INT,
    @Tipo      VARCHAR(20),
    @Fecha     DATE,
    @Concepto  VARCHAR(255),
    @MedioPago VARCHAR(20),
    @Monto     DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Tipo      = UPPER(TRIM(@Tipo));
    SET @Concepto  = TRIM(@Concepto);
    SET @MedioPago = UPPER(TRIM(@MedioPago));

    IF @Tipo IS NULL OR @Tipo <> 'SERVICIOS'
        THROW 50001, 'Por ahora solo se pueden registrar egresos de tipo Servicios.', 1;

    IF @Fecha IS NULL
        THROW 50002, 'La fecha es obligatoria.', 1;

    IF NULLIF(@Concepto, '') IS NULL
        THROW 50003, 'El concepto es obligatorio.', 1;

    IF @MedioPago IS NULL OR @MedioPago NOT IN ('EFECTIVO', 'TRANSFERENCIA')
        THROW 50004, 'El medio de pago no es válido.', 1;

    IF @Monto IS NULL OR @Monto <= 0
        THROW 50005, 'El monto debe ser mayor que cero.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE IdUsuario = @IdUsuario)
        THROW 50006, 'El usuario no existe.', 1;

    INSERT INTO dbo.Egresos (IdUsuario, Tipo, Fecha, Concepto, MedioPago, Monto)
    VALUES (@IdUsuario, @Tipo, @Fecha, @Concepto, @MedioPago, @Monto);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS IdEgreso;
END
GO



CREATE PROCEDURE api.usp_Egresos_Listar
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
    ORDER BY Fecha DESC, IdEgreso DESC
END
GO


