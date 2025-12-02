IF OBJECT_ID('dbo.usp_CreateBasket', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_CreateBasket;
END
GO
CREATE PROCEDURE dbo.usp_CreateBasket
    @Id UNIQUEIDENTIFIER,
    @DiscountDefinitionId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Baskets (
        Id,
        DiscountDefinitionId,
        CreatedAt,
        ModifiedAt)
    VALUES (
        @Id,
        @DiscountDefinitionId,
        SYSUTCDATETIME(),
        SYSUTCDATETIME());
END
GO

