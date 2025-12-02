IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo;');
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'ShippingCosts'
)
BEGIN
    CREATE TABLE dbo.ShippingCosts (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Country NVARCHAR(200) NOT NULL,
        Cost INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        ModifiedAt DATETIME2 NOT NULL,
        CONSTRAINT UQ_ShippingCosts_Country UNIQUE (Country)
    );
END