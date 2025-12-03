IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo;');
END

IF OBJECT_ID('dbo.Items', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Items (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        BasketId UNIQUEIDENTIFIER NOT NULL,
        ProductId INT NOT NULL IDENTITY(1,1),
        Name NVARCHAR(200) NOT NULL,
        UnitPrice INT NOT NULL,
        Quantity INT NOT NULL,
        ItemDiscountType TINYINT NULL,
        ItemDiscountAmount INT NULL,
        CreatedAt DATETIME2 NOT NULL,
        ModifiedAt DATETIME2 NOT NULL,
        CONSTRAINT FK_Items_Baskets FOREIGN KEY (BasketId) REFERENCES dbo.Baskets(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_Items_BasketId ON dbo.Items (BasketId);
END

