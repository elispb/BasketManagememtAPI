IF OBJECT_ID('dbo.DiscountDefinitions', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.DiscountDefinitions WHERE Code = 'VACAY10')
    BEGIN
        INSERT INTO dbo.DiscountDefinitions (Id, Code, Percentage, Metadata, IsActive, CreatedAt, ModifiedAt)
        VALUES (NEWID(), 'VACAY10', 10, '{"source":"seed","description":"Seasonal travel promo"}', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.DiscountDefinitions WHERE Code = 'WELCOME15')
    BEGIN
        INSERT INTO dbo.DiscountDefinitions (Id, Code, Percentage, Metadata, IsActive, CreatedAt, ModifiedAt)
        VALUES (NEWID(), 'WELCOME15', 15, '{"source":"seed","description":"New customer welcome"}', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
    END
END

