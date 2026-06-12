/*
    AuthDb — dodavanje kolone LastName (prezime)
*/

USE AuthDb;
GO

IF COL_LENGTH(N'dbo.Users', N'LastName') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD LastName NVARCHAR(100) NOT NULL CONSTRAINT DF_Users_LastName DEFAULT (N'');
END
GO

PRINT N'Kolona LastName je dodata u Users.';
GO
