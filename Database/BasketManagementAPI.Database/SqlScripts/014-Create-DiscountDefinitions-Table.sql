IF OBJECT_ID('dbo.DiscountDefinitions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DiscountDefinitions (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Code NVARCHAR(100) NOT NULL UNIQUE,
        Percentage DECIMAL(5,2) NULL,
        Metadata NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL,
        ModifiedAt DATETIME2 NOT NULL
    );
END

IF OBJECT_ID('FK_Baskets_DiscountDefinition', 'F') IS NULL
    AND OBJECT_ID('dbo.DiscountDefinitions', 'U') IS NOT NULL
    AND EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.Baskets')
          AND name = 'DiscountDefinitionId'
    )
BEGIN
    ALTER TABLE dbo.Baskets
    ADD CONSTRAINT FK_Baskets_DiscountDefinition
        FOREIGN KEY (DiscountDefinitionId)
        REFERENCES dbo.DiscountDefinitions(Id)
        ON DELETE SET NULL;
END

