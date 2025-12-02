IF OBJECT_ID('dbo.usp_UpdateDiscountDefinition', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_UpdateDiscountDefinition;
END
GO
CREATE PROCEDURE dbo.usp_UpdateDiscountDefinition
    @Id UNIQUEIDENTIFIER,
    @Code NVARCHAR(100),
    @Percentage DECIMAL(5,2) = NULL,
    @Metadata NVARCHAR(MAX) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.DiscountDefinitions
    SET Code = @Code,
        Percentage = @Percentage,
        Metadata = @Metadata,
        IsActive = @IsActive,
        ModifiedAt = SYSUTCDATETIME()
    WHERE Id = @Id;
END
GO

