IF OBJECT_ID('dbo.usp_UpsertBasketShipping', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_UpsertBasketShipping;
END
GO
CREATE PROCEDURE dbo.usp_UpsertBasketShipping
    @BasketId UNIQUEIDENTIFIER,
    @Country NVARCHAR(200),
    @Cost INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.BasketShipping WHERE BasketId = @BasketId)
    BEGIN
        UPDATE dbo.BasketShipping
        SET Country = @Country,
            Cost = @Cost,
            ModifiedAt = SYSUTCDATETIME()
        WHERE BasketId = @BasketId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.BasketShipping (
            BasketId,
            Country,
            Cost,
            CreatedAt,
            ModifiedAt)
        VALUES (
            @BasketId,
            @Country,
            @Cost,
            SYSUTCDATETIME(),
            SYSUTCDATETIME());
    END
END
GO

