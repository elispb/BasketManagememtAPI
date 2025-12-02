IF OBJECT_ID('dbo.usp_DeactivateDiscountDefinition', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_DeactivateDiscountDefinition;
END
GO
CREATE PROCEDURE dbo.usp_DeactivateDiscountDefinition
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.DiscountDefinitions
    SET IsActive = 0,
        ModifiedAt = SYSUTCDATETIME()
    WHERE Id = @Id;
END
GO

