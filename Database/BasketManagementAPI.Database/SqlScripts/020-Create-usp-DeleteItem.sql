IF OBJECT_ID('dbo.usp_DeleteItem', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_DeleteItem;
END
GO
CREATE PROCEDURE dbo.usp_DeleteItem
    @BasketId UNIQUEIDENTIFIER,
    @ProductId NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Items
    WHERE BasketId = @BasketId
      AND ProductId = @ProductId;
    RETURN @@ROWCOUNT;
END
GO

