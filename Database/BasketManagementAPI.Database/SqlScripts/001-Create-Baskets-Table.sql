IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo;');
END

IF OBJECT_ID('dbo.Baskets', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Baskets (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        DiscountDefinitionId UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL,
        ModifiedAt DATETIME2 NOT NULL
    );
END
