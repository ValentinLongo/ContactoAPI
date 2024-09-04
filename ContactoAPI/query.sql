-- Creación de la Base de Datos
CREATE DATABASE dbContacto;
GO

USE dbContacto;
GO

-- Creación de la tabla Contacto
CREATE TABLE Contacto (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) NOT NULL,
    Nombres NVARCHAR(100) NOT NULL,
    Apellidos NVARCHAR(100) NOT NULL,
    Comentarios NVARCHAR(1000) NOT NULL,
    Adjunto VARBINARY(MAX),
    AdjuntoNombre NVARCHAR(255),
    FechaEnvio DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Creación del SP para insertar un contacto
CREATE PROCEDURE InsertarContacto
    @Email NVARCHAR(255),
    @Nombres NVARCHAR(100),
    @Apellidos NVARCHAR(100),
    @Comentarios NVARCHAR(1000),
    @Adjunto VARBINARY(MAX) = NULL,
    @AdjuntoNombre NVARCHAR(255) = NULL
AS
BEGIN
    INSERT INTO Contacto (Email, Nombres, Apellidos, Comentarios, Adjunto, AdjuntoNombre)
    VALUES (@Email, @Nombres, @Apellidos, @Comentarios, @Adjunto, @AdjuntoNombre);

    SELECT SCOPE_IDENTITY() AS Id;
END;
GO

--Creación del SP para Obtener Contactos
CREATE PROCEDURE ObtenerContactos
AS
BEGIN
    SELECT 
        Id, 
        Email, 
        Nombres, 
        Apellidos, 
        Comentarios, 
        Adjunto,
        AdjuntoNombre, 
        FechaEnvio
    FROM 
        Contacto;
END;
GO

CREATE PROCEDURE ObtenerContactoPorId
    @Id INT
AS
BEGIN
    SELECT 
        Id, 
        Email, 
        Nombres, 
        Apellidos, 
        Comentarios, 
        AdjuntoNombre, 
        FechaEnvio
    FROM 
        Contacto
    WHERE 
        Id = @Id;
END;
GO

CREATE PROCEDURE ActualizarContacto
    @Id INT,
    @Email NVARCHAR(255),
    @Nombres NVARCHAR(100),
    @Apellidos NVARCHAR(100),
    @Comentarios NVARCHAR(1000),
    @Adjunto VARBINARY(MAX) = NULL,
    @AdjuntoNombre NVARCHAR(255) = NULL
AS
BEGIN
    UPDATE Contacto
    SET 
        Email = @Email,
        Nombres = @Nombres,
        Apellidos = @Apellidos,
        Comentarios = @Comentarios,
        Adjunto = @Adjunto,
        AdjuntoNombre = @AdjuntoNombre,
        FechaEnvio = GETDATE()
    WHERE 
        Id = @Id;
END;
GO

CREATE PROCEDURE EliminarContacto
    @Id INT
AS
BEGIN
    DELETE FROM Contacto
    WHERE 
        Id = @Id;
END;
GO