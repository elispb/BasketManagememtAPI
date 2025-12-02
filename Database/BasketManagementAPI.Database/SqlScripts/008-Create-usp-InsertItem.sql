IF OBJECT_ID('dbo.usp_InsertItem', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_InsertItem;
END
GO
CREATE PROCEDURE dbo.usp_InsertItem
    @Id UNIQUEIDENTIFIER,
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
        @Id,
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
GO

