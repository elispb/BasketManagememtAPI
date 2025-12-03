IF OBJECT_ID('dbo.usp_UpdateItem', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_UpdateItem;
END
GO
CREATE PROCEDURE dbo.usp_UpdateItem
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @UnitPrice INT,
    @Quantity INT,
    @ItemDiscountType TINYINT = NULL,
    @ItemDiscountAmount INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Items
    SET Name = @Name,
        UnitPrice = @UnitPrice,
        Quantity = @Quantity,
        ItemDiscountType = @ItemDiscountType,
        ItemDiscountAmount = @ItemDiscountAmount,
        ModifiedAt = SYSUTCDATETIME()
    WHERE Id = @Id;
END
GO

