IF OBJECT_ID('dbo.usp_DeleteItemsForBasket', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_DeleteItemsForBasket;
END
GO
CREATE PROCEDURE dbo.usp_DeleteItemsForBasket
    @BasketId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Items
    WHERE BasketId = @BasketId;
END
GO

