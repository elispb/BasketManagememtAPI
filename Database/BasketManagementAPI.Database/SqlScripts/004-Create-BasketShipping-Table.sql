IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo;');
END

IF OBJECT_ID('dbo.BasketShipping', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BasketShipping (
        BasketId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        CountryCode NVARCHAR(10) NOT NULL,
        Cost INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        ModifiedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_BasketShipping_Baskets FOREIGN KEY (BasketId) REFERENCES dbo.Baskets(Id) ON DELETE CASCADE
    );
END

