IF OBJECT_ID('dbo.usp_InsertDiscountDefinition', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_InsertDiscountDefinition;
END
GO
CREATE PROCEDURE dbo.usp_InsertDiscountDefinition
    @Id UNIQUEIDENTIFIER,
    @Code NVARCHAR(100),
    @Percentage DECIMAL(5,2) = NULL,
    @Metadata NVARCHAR(MAX) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.DiscountDefinitions (
        Id,
        Code,
        Percentage,
        Metadata,
        IsActive,
        CreatedAt,
        ModifiedAt)
    VALUES (
        @Id,
        @Code,
        @Percentage,
        @Metadata,
        @IsActive,
        SYSUTCDATETIME(),
        SYSUTCDATETIME());
END
GO

