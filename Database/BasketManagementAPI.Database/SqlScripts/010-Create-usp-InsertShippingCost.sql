IF OBJECT_ID('dbo.usp_InsertShippingCost', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_InsertShippingCost;
END
GO
CREATE PROCEDURE dbo.usp_InsertShippingCost
    @Id UNIQUEIDENTIFIER,
    @Country NVARCHAR(200),
    @Cost INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ShippingCosts (
        Id,
        Country,
        Cost,
        CreatedAt,
        ModifiedAt)
    VALUES (
        @Id,
        @Country,
        @Cost,
        SYSUTCDATETIME(),
        SYSUTCDATETIME());
END
GO

