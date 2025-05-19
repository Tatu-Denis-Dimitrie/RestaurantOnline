-- Șterge trigger-ul existent
DROP TRIGGER IF EXISTS tr_UpdateDishStock;
GO

-- Creează noul trigger care gestionează atât actualizarea stocului cât și anularea comenzilor
CREATE TRIGGER tr_UpdateDishStock
ON Orders
AFTER UPDATE
AS
BEGIN
    -- Cazul 1: Comandă trecută în starea 'preparing' - scade stocul
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN deleted d ON i.OrderId = d.OrderId
        WHERE i.Status = 'preparing' AND d.Status <> 'preparing'
    )
    BEGIN
        UPDATE d
        SET d.TotalQuantityGrams = d.TotalQuantityGrams - (od.Quantity * dish.PortionSizeGrams)
        FROM Dishes d
        JOIN OrderDish od ON d.DishId = od.DishId
        JOIN inserted i ON od.OrderId = i.OrderId
        JOIN Dishes dish ON dish.DishId = od.DishId;
    END

    -- Cazul 2: Comandă anulată - restaurează stocul
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN deleted d ON i.OrderId = d.OrderId
        WHERE i.Status = 'anulata' AND d.Status <> 'anulata'
    )
    BEGIN
        UPDATE d
        SET d.TotalQuantityGrams = d.TotalQuantityGrams + (od.Quantity * dish.PortionSizeGrams)
        FROM Dishes d
        JOIN OrderDish od ON d.DishId = od.DishId
        JOIN inserted i ON od.OrderId = i.OrderId
        JOIN Dishes dish ON dish.DishId = od.DishId;
    END
END;
GO

-- Procedură stocată pentru a anula o comandă (dezactivează și reactivează trigger-ul)
CREATE OR ALTER PROCEDURE CancelOrder
    @OrderId INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Dezactivăm trigger-ul temporar
        DISABLE TRIGGER tr_UpdateDishStock ON Orders;
        
        -- Actualizăm starea comenzii
        UPDATE Orders
        SET Status = 'anulata'
        WHERE OrderId = @OrderId;
        
        -- Actualizăm manual stocul
        UPDATE d
        SET d.TotalQuantityGrams = d.TotalQuantityGrams + (od.Quantity * d.PortionSizeGrams)
        FROM Dishes d
        JOIN OrderDish od ON d.DishId = od.DishId
        WHERE od.OrderId = @OrderId;
        
        -- Reactivăm trigger-ul
        ENABLE TRIGGER tr_UpdateDishStock ON Orders;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        -- Ne asigurăm că trigger-ul este reactivat în caz de eroare
        ENABLE TRIGGER tr_UpdateDishStock ON Orders;
        
        -- Returnăm eroarea
        THROW;
    END CATCH
END;
GO

-- Procedură stocată pentru actualizarea statusului oricărei comenzi
CREATE OR ALTER PROCEDURE UpdateOrderStatus
    @OrderId INT,
    @NewStatus NVARCHAR(50)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Dezactivăm trigger-ul temporar
        DISABLE TRIGGER tr_UpdateDishStock ON Orders;
        
        -- Actualizăm starea comenzii
        UPDATE Orders
        SET Status = @NewStatus
        WHERE OrderId = @OrderId;
        
        -- Dacă statusul este 'se_pregateste' (echivalent cu 'preparing'), actualizăm stocul
        IF @NewStatus = 'se_pregateste'
        BEGIN
            UPDATE d
            SET d.TotalQuantityGrams = d.TotalQuantityGrams - (od.Quantity * d.PortionSizeGrams)
            FROM Dishes d
            JOIN OrderDish od ON d.DishId = od.DishId
            WHERE od.OrderId = @OrderId;
        END
        
        -- Dacă statusul este 'anulata', restaurăm stocul
        IF @NewStatus = 'anulata'
        BEGIN
            UPDATE d
            SET d.TotalQuantityGrams = d.TotalQuantityGrams + (od.Quantity * d.PortionSizeGrams)
            FROM Dishes d
            JOIN OrderDish od ON d.DishId = od.DishId
            WHERE od.OrderId = @OrderId;
        END
        
        -- Reactivăm trigger-ul
        ENABLE TRIGGER tr_UpdateDishStock ON Orders;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        -- Ne asigurăm că trigger-ul este reactivat în caz de eroare
        ENABLE TRIGGER tr_UpdateDishStock ON Orders;
        
        -- Returnăm eroarea
        THROW;
    END CATCH
END;
GO 