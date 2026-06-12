/*
    BudgetService baza — samo troškovi
    TravelPlanId je logička referenca na TravelDb (bez FK između baza).
*/

USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'BudgetDb')
BEGIN
    CREATE DATABASE BudgetDb;
END
GO

USE BudgetDb;
GO

IF OBJECT_ID(N'dbo.Expenses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Expenses
    (
        Id             INT IDENTITY(1, 1) NOT NULL,
        TravelPlanId   INT                NOT NULL,
        Name           NVARCHAR(200)      NOT NULL,
        Category       NVARCHAR(50)       NOT NULL,
        Amount         DECIMAL(18, 2)     NOT NULL,
        ExpenseDate    DATE               NOT NULL,
        Description    NVARCHAR(MAX)      NULL,

        CONSTRAINT PK_Expenses PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Expenses_Amount CHECK (Amount >= 0),
        CONSTRAINT CK_Expenses_Category CHECK (Category IN (
            N'Transport',
            N'Accommodation',
            N'Food',
            N'Tickets',
            N'Shopping',
            N'Other'
        ))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Expenses_TravelPlanId' AND object_id = OBJECT_ID(N'dbo.Expenses'))
    CREATE NONCLUSTERED INDEX IX_Expenses_TravelPlanId ON dbo.Expenses (TravelPlanId);
GO

PRINT N'BudgetDb je spremna.';
GO
