-- Добавляем чай
INSERT INTO Teas (Id, Name, CurrentStock) 
VALUES ('ae88e1da-45e3-4903-a241-944365399582', 'Габа Алишань', 500.0);

-- Устанавливаем цену (50 руб за грамм для клуба)
INSERT INTO TeaPrices (TeaId, BrewPricePerGram, SalePricePerGram)
VALUES ('ae88e1da-45e3-4903-a241-944365399582', 50.0, 35.0);

-- Создаем клиента с балансом 2000 руб
INSERT INTO Users (Id, FullName, Role, Balance)
VALUES ('7488f72a-6056-4299-8798-8255964f4342', 'Иван Чайный', 'Customer', 2000.0);


SELECT * FROM Visits WHERE IsClosed = false

SELECT * FROM Teas