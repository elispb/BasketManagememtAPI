IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo;');
END

IF OBJECT_ID('dbo.ShippingCosts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShippingCosts (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Country NVARCHAR(200) NOT NULL,
        CountryCode INT NOT NULL,
        Cost INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        ModifiedAt DATETIME2 NOT NULL,
        CONSTRAINT UQ_ShippingCosts_Country UNIQUE (Country),
        CONSTRAINT UQ_ShippingCosts_CountryCode UNIQUE (CountryCode)
    );
END

