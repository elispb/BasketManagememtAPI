IF OBJECT_ID('dbo.ShippingCosts', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.ShippingCosts WHERE Country = 'United Kingdom')
    BEGIN
        INSERT INTO dbo.ShippingCosts (Id, Country, CountryCode, Cost, CreatedAt, ModifiedAt)
        VALUES (NEWID(), 'United Kingdom', 1, 499, SYSUTCDATETIME(), SYSUTCDATETIME());
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.ShippingCosts WHERE Country = 'Germany')
    BEGIN
        INSERT INTO dbo.ShippingCosts (Id, Country, CountryCode, Cost, CreatedAt, ModifiedAt)
        VALUES (NEWID(), 'Germany', 2, 699, SYSUTCDATETIME(), SYSUTCDATETIME());
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.ShippingCosts WHERE Country = 'United States')
    BEGIN
        INSERT INTO dbo.ShippingCosts (Id, Country, CountryCode, Cost, CreatedAt, ModifiedAt)
        VALUES (NEWID(), 'United States', 3, 1299, SYSUTCDATETIME(), SYSUTCDATETIME());
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.ShippingCosts WHERE Country = 'Australia')
    BEGIN
        INSERT INTO dbo.ShippingCosts (Id, Country, CountryCode, Cost, CreatedAt, ModifiedAt)
        VALUES (NEWID(), 'Australia', 4, 1399, SYSUTCDATETIME(), SYSUTCDATETIME());
    END
END

