IF OBJECT_ID('dbo.usp_UpdateItemDiscount', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_UpdateItemDiscount;
END
GO
CREATE PROCEDURE dbo.usp_UpdateItemDiscount
    @BasketId UNIQUEIDENTIFIER,
    @ProductId NVARCHAR(100),
    @ItemDiscountType TINYINT = NULL,
    @ItemDiscountAmount INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Items
    SET ItemDiscountType = @ItemDiscountType,
        ItemDiscountAmount = @ItemDiscountAmount,
        ModifiedAt = SYSUTCDATETIME()
    OUTPUT
        inserted.ProductId,
        inserted.Name,
        inserted.UnitPrice,
        inserted.Quantity,
        inserted.ItemDiscountType,
        inserted.ItemDiscountAmount
    WHERE BasketId = @BasketId
      AND ProductId = @ProductId;
END
GO

