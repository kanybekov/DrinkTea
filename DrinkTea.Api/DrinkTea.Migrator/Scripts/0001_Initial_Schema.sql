
-- 1. ТАБЛИЦА ПОЛЬЗОВАТЕЛЕЙ (Мастера и Клиенты)
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    FullName TEXT NOT NULL,
    Login TEXT UNIQUE,
    PasswordHash TEXT,
    Role INTEGER NOT NULL DEFAULT 0, -- 0: Customer, 1: Master
    Balance DECIMAL(12, 2) NOT NULL DEFAULT 0,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 2. ТАБЛИЦА ЧАЯ (Склад)
CREATE TABLE Teas (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name TEXT NOT NULL,
    CurrentStock DECIMAL(12, 2) NOT NULL DEFAULT 0 -- Вес строго в граммах
);

-- 3. ИСТОРИЯ ЦЕН (Для версионирования)
CREATE TABLE TeaPrices (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TeaId UUID NOT NULL REFERENCES Teas(Id) ON DELETE CASCADE,
    BrewPricePerGram DECIMAL(12, 2) NOT NULL, -- Цена заварки в клубе
    SalePricePerGram DECIMAL(12, 2) NOT NULL, -- Цена продажи пачки
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 4. ВИЗИТЫ (Открытые счета)
CREATE TABLE Visits (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES Users(Id), -- NULL для анонимов
    Note TEXT,                       -- Метка для анонима ("Стол 5")
    StartTime TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    EndTime TIMESTAMP WITH TIME ZONE,
    TotalAmount DECIMAL(12, 2) NOT NULL DEFAULT 0, -- Текущий долг за чай
    IsClosed BOOLEAN NOT NULL DEFAULT FALSE
);

-- 5. СЕССИИ ЗАВАРИВАНИЯ (Посиделки)
CREATE TABLE BrewingSessions (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TeaId UUID NOT NULL REFERENCES Teas(Id),
    PriceSnapshotId UUID NOT NULL REFERENCES TeaPrices(Id),
    StaffId UUID NOT NULL REFERENCES Users(Id), -- Кто заварил
    TotalGrams DECIMAL(12, 2) NOT NULL,
    TotalCost DECIMAL(12, 2) NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 6. УЧАСТНИКИ ЗАВАРИВАНИЯ (Связь сессии и визита)
CREATE TABLE BrewingParticipants (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    SessionId UUID NOT NULL REFERENCES BrewingSessions(Id) ON DELETE CASCADE,
    VisitId UUID NOT NULL REFERENCES Visits(Id) ON DELETE CASCADE,
    ShareCost DECIMAL(12, 2) NOT NULL -- Доля этого гостя
);

-- 7. РОЗНИЧНЫЕ ПРОДАЖИ (Магазин)
CREATE TABLE Sales (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TeaId UUID NOT NULL REFERENCES Teas(Id),
    UserId UUID REFERENCES Users(Id), -- Кто купил
    StaffId UUID NOT NULL REFERENCES Users(Id), -- Кто продал
    Grams DECIMAL(12, 2) NOT NULL,
    TotalCost DECIMAL(12, 2) NOT NULL,
    PaymentMethod INTEGER NOT NULL, -- 1: Internal, 2: Cash, 3: Card
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 8. ФИНАНСОВЫЕ ТРАНЗАКЦИИ (Главный лог денег)
CREATE TABLE Transactions (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    VisitId UUID REFERENCES Visits(Id), -- Если оплата визита
    UserId UUID REFERENCES Users(Id),  -- Кто платил
    StaffId UUID NOT NULL REFERENCES Users(Id), -- Кто принял деньги
    Amount DECIMAL(12, 2) NOT NULL,
    PaymentMethod INTEGER NOT NULL, -- 1: Internal, 2: Cash, 3: Card
    Description TEXT, -- "Розничная продажа: Габа", "Оплата визита" и т.д.
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_visits_active ON Visits(IsClosed) WHERE IsClosed = FALSE;
CREATE INDEX idx_transactions_user ON Transactions(UserId);


-- Создаем первого Админа (пароль: admin123)
INSERT INTO Users (FullName, Login, PasswordHash, Role)
VALUES ('Главный Мастер', 'admin', '$2a$11$ev7.S.n8J8J0X/3C7v9GueZfI5rA5R8f5Y5F/I5z1mQyP9K7X8k2G', 1);

INSERT INTO Users (Id, FullName, Login, PasswordHash, Role, Balance)
VALUES (gen_random_uuid(), 'Иван Иванов', 'ivan_tea', NULL, 0, 5000.00);


-- 1. Добавляем недостающее поле в сессии заваривания
ALTER TABLE BrewingSessions 
ADD COLUMN IsFinished BOOLEAN NOT NULL DEFAULT FALSE;

-- 2. Добавляем поле единицы измерения в таблицу чая (как в модели Tea.cs)
ALTER TABLE Teas 
ADD COLUMN Unit TEXT NOT NULL DEFAULT 'g';

-- 3. Создаем индексы для ускорения финансовых отчетов
CREATE INDEX idx_transactions_createdat ON Transactions(CreatedAt);
CREATE INDEX idx_sales_createdat ON Sales(CreatedAt);

-- 4. Индекс для быстрого поиска активных сессий (используется в GetActiveSessions)
CREATE INDEX idx_brewing_active ON BrewingSessions(IsFinished) WHERE IsFinished = FALSE;
