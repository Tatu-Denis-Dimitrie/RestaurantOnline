-- Procedură stocată pentru actualizarea unui preparat și a alergenilor asociați
CREATE OR ALTER PROCEDURE sp_UpdateDishWithAllergens
    @DishId INT,
    @Name NVARCHAR(100),
    @Price DECIMAL(10, 2),
    @PortionSizeGrams INT,
    @TotalQuantityGrams INT,
    @CategoryId INT,
    @AllergenIds NVARCHAR(MAX) -- Format: '1,2,3,4' string de ID-uri separate prin virgulă
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- 1. Actualizăm informațiile de bază ale preparatului
        UPDATE Dishes
        SET Name = @Name,
            Price = @Price,
            PortionSizeGrams = @PortionSizeGrams,
            TotalQuantityGrams = @TotalQuantityGrams,
            CategoryId = @CategoryId
        WHERE DishId = @DishId;
        
        -- 2. Ștergem toate relațiile existente între preparat și alergeni
        DELETE FROM DishAllergen WHERE DishId = @DishId;
        
        -- 3. Inserăm noile relații cu alergenii
        -- Convertim string-ul în tabel temporar
        IF @AllergenIds IS NOT NULL AND LEN(@AllergenIds) > 0
        BEGIN
            CREATE TABLE #TempAllergens (AllergenId INT);
            
            INSERT INTO #TempAllergens (AllergenId)
            SELECT value FROM STRING_SPLIT(@AllergenIds, ',');
            
            -- Inserăm noile relații
            INSERT INTO DishAllergen (DishId, AllergenId)
            SELECT @DishId, AllergenId FROM #TempAllergens;
            
            DROP TABLE #TempAllergens;
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Procedură stocată pentru actualizarea imaginii unui preparat
CREATE OR ALTER PROCEDURE sp_UpdateDishPhoto
    @DishId INT,
    @PhotoUrl NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificăm dacă există deja o poză pentru acest preparat
        DECLARE @PhotoId INT = NULL;
        SELECT TOP 1 @PhotoId = PhotoId FROM DishPhotos WHERE DishId = @DishId;
        
        IF @PhotoId IS NOT NULL
        BEGIN
            -- Actualizăm poza existentă
            UPDATE DishPhotos SET Url = @PhotoUrl WHERE PhotoId = @PhotoId;
        END
        ELSE
        BEGIN
            -- Adăugăm o poză nouă
            INSERT INTO DishPhotos (DishId, Url) VALUES (@DishId, @PhotoUrl);
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END 