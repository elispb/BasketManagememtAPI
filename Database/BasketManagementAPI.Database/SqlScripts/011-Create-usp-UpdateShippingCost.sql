IF OBJECT_ID('dbo.usp_UpdateShippingCost', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_UpdateShippingCost;
END
GO
CREATE PROCEDURE dbo.usp_UpdateShippingCost
    @Country NVARCHAR(200),
    @Cost INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ShippingCosts
    SET Cost = @Cost,
        ModifiedAt = SYSUTCDATETIME()
    WHERE Country = @Country;
END
GO

