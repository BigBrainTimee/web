/*
    AuthService baza — samo korisnici
*/

USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'AuthDb')
BEGIN
    CREATE DATABASE AuthDb;
END
GO

USE AuthDb;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id            INT IDENTITY(1, 1) NOT NULL,
        Name          NVARCHAR(100)      NOT NULL,
        LastName      NVARCHAR(100)      NOT NULL CONSTRAINT DF_Users_LastName DEFAULT (N''),
        Email         NVARCHAR(256)      NOT NULL,
        PasswordHash  NVARCHAR(512)      NOT NULL,
        Role          NVARCHAR(20)       NOT NULL,
        CreatedAt     DATETIME2(0)       NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'User', N'Admin'))
    );
END
GO

PRINT N'AuthDb je spremna.';
GO
