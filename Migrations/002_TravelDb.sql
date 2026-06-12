/*
    TravelService baza — planovi, destinacije, aktivnosti, checklist, share linkovi
    UserId je logička referenca na AuthDb (bez FK između baza).
*/

USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'TravelDb')
BEGIN
    CREATE DATABASE TravelDb;
END
GO

USE TravelDb;
GO

IF OBJECT_ID(N'dbo.TravelPlans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TravelPlans
    (
        Id             INT IDENTITY(1, 1) NOT NULL,
        UserId         INT                NOT NULL,
        Name           NVARCHAR(200)      NOT NULL,
        Description    NVARCHAR(MAX)      NULL,
        StartDate      DATE               NOT NULL,
        EndDate        DATE               NOT NULL,
        PlannedBudget  DECIMAL(18, 2)     NOT NULL,
        Notes          NVARCHAR(MAX)      NULL,
        CreatedAt      DATETIME2(0)       NOT NULL CONSTRAINT DF_TravelPlans_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt      DATETIME2(0)       NOT NULL CONSTRAINT DF_TravelPlans_UpdatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_TravelPlans PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_TravelPlans_Dates CHECK (EndDate >= StartDate),
        CONSTRAINT CK_TravelPlans_PlannedBudget CHECK (PlannedBudget >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.Destinations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Destinations
    (
        Id             INT IDENTITY(1, 1) NOT NULL,
        TravelPlanId   INT                NOT NULL,
        Name           NVARCHAR(200)      NOT NULL,
        Location       NVARCHAR(300)      NOT NULL,
        ArrivalDate    DATE               NOT NULL,
        DepartureDate  DATE               NOT NULL,
        Description    NVARCHAR(MAX)      NULL,

        CONSTRAINT PK_Destinations PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Destinations_TravelPlans FOREIGN KEY (TravelPlanId)
            REFERENCES dbo.TravelPlans (Id) ON DELETE CASCADE,
        CONSTRAINT CK_Destinations_Dates CHECK (DepartureDate >= ArrivalDate)
    );
END
GO

IF OBJECT_ID(N'dbo.Activities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Activities
    (
        Id              INT IDENTITY(1, 1) NOT NULL,
        TravelPlanId    INT                NOT NULL,
        DestinationId   INT                NULL,
        ActivityDate    DATE               NOT NULL,
        ActivityTime    TIME(0)            NULL,
        Name            NVARCHAR(200)      NOT NULL,
        Location        NVARCHAR(300)      NULL,
        Description     NVARCHAR(MAX)      NULL,
        EstimatedCost   DECIMAL(18, 2)     NULL,
        Status          NVARCHAR(20)       NOT NULL,

        CONSTRAINT PK_Activities PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Activities_TravelPlans FOREIGN KEY (TravelPlanId)
            REFERENCES dbo.TravelPlans (Id) ON DELETE CASCADE,
        CONSTRAINT FK_Activities_Destinations FOREIGN KEY (DestinationId)
            REFERENCES dbo.Destinations (Id) ON DELETE NO ACTION,
        CONSTRAINT CK_Activities_Status CHECK (Status IN (N'Planned', N'Reserved', N'Completed', N'Cancelled')),
        CONSTRAINT CK_Activities_EstimatedCost CHECK (EstimatedCost IS NULL OR EstimatedCost >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.ChecklistItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChecklistItems
    (
        Id             INT IDENTITY(1, 1) NOT NULL,
        TravelPlanId   INT                NOT NULL,
        Title          NVARCHAR(300)      NOT NULL,
        IsCompleted    BIT                NOT NULL CONSTRAINT DF_ChecklistItems_IsCompleted DEFAULT (0),
        SortOrder      INT                NOT NULL CONSTRAINT DF_ChecklistItems_SortOrder DEFAULT (0),

        CONSTRAINT PK_ChecklistItems PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_ChecklistItems_TravelPlans FOREIGN KEY (TravelPlanId)
            REFERENCES dbo.TravelPlans (Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.ShareLinks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShareLinks
    (
        Id             INT IDENTITY(1, 1) NOT NULL,
        TravelPlanId   INT                NOT NULL,
        Token          NVARCHAR(100)      NOT NULL,
        AccessType     NVARCHAR(10)       NOT NULL,
        CreatedAt      DATETIME2(0)       NOT NULL CONSTRAINT DF_ShareLinks_CreatedAt DEFAULT (SYSUTCDATETIME()),
        ExpiresAt      DATETIME2(0)       NULL,

        CONSTRAINT PK_ShareLinks PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_ShareLinks_Token UNIQUE (Token),
        CONSTRAINT FK_ShareLinks_TravelPlans FOREIGN KEY (TravelPlanId)
            REFERENCES dbo.TravelPlans (Id) ON DELETE CASCADE,
        CONSTRAINT CK_ShareLinks_AccessType CHECK (AccessType IN (N'View', N'Edit'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TravelPlans_UserId' AND object_id = OBJECT_ID(N'dbo.TravelPlans'))
    CREATE NONCLUSTERED INDEX IX_TravelPlans_UserId ON dbo.TravelPlans (UserId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Destinations_TravelPlanId' AND object_id = OBJECT_ID(N'dbo.Destinations'))
    CREATE NONCLUSTERED INDEX IX_Destinations_TravelPlanId ON dbo.Destinations (TravelPlanId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Activities_TravelPlanId_ActivityDate' AND object_id = OBJECT_ID(N'dbo.Activities'))
    CREATE NONCLUSTERED INDEX IX_Activities_TravelPlanId_ActivityDate ON dbo.Activities (TravelPlanId, ActivityDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChecklistItems_TravelPlanId' AND object_id = OBJECT_ID(N'dbo.ChecklistItems'))
    CREATE NONCLUSTERED INDEX IX_ChecklistItems_TravelPlanId ON dbo.ChecklistItems (TravelPlanId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ShareLinks_TravelPlanId' AND object_id = OBJECT_ID(N'dbo.ShareLinks'))
    CREATE NONCLUSTERED INDEX IX_ShareLinks_TravelPlanId ON dbo.ShareLinks (TravelPlanId);
GO

PRINT N'TravelDb je spremna.';
GO
