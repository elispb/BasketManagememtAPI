IF OBJECT_ID('dbo.usp_UpdateBasket', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_UpdateBasket;
END
GO
CREATE PROCEDURE dbo.usp_UpdateBasket
    @Id UNIQUEIDENTIFIER,
    @DiscountDefinitionId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Baskets
    SET DiscountDefinitionId = @DiscountDefinitionId,
        ModifiedAt = SYSUTCDATETIME()
    WHERE Id = @Id;
END
GO

