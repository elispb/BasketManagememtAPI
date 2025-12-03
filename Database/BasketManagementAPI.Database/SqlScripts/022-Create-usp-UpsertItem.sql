IF OBJECT_ID('dbo.usp_UpsertItem', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_UpsertItem;
END
GO
CREATE PROCEDURE dbo.usp_UpsertItem
    @BasketId UNIQUEIDENTIFIER,
    @ProductId NVARCHAR(100),
    @Name NVARCHAR(200),
    @UnitPrice INT,
    @Quantity INT,
    @ItemDiscountType TINYINT = NULL,
    @ItemDiscountAmount INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM dbo.Items
        WHERE BasketId = @BasketId
          AND ProductId = @ProductId)
    BEGIN
        UPDATE dbo.Items
        SET Name = @Name,
            UnitPrice = @UnitPrice,
            Quantity = @Quantity,
            ItemDiscountType = @ItemDiscountType,
            ItemDiscountAmount = @ItemDiscountAmount,
            ModifiedAt = SYSUTCDATETIME()
        WHERE BasketId = @BasketId
          AND ProductId = @ProductId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Items (
            Id,
            BasketId,
            ProductId,
            Name,
            UnitPrice,
            Quantity,
            ItemDiscountType,
            ItemDiscountAmount,
            CreatedAt,
            ModifiedAt)
        VALUES (
            NEWID(),
            @BasketId,
            @ProductId,
            @Name,
            @UnitPrice,
            @Quantity,
            @ItemDiscountType,
            @ItemDiscountAmount,
            SYSUTCDATETIME(),
            SYSUTCDATETIME());
    END
END
GO

