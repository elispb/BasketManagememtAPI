IF OBJECT_ID('dbo.usp_DeleteBasketShipping', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_DeleteBasketShipping;
END
GO
CREATE PROCEDURE dbo.usp_DeleteBasketShipping
    @BasketId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.BasketShipping
    WHERE BasketId = @BasketId;
END
GO

