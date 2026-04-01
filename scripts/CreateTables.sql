-- 1. Пользователи (Мастера и Постоянщики)
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    FullName TEXT NOT NULL,
    TelegramId BIGINT UNIQUE, -- Для бота
    PasswordHash TEXT,        -- Только для мастеров
    Role TEXT NOT NULL,       -- Admin, Staff, Customer
    Balance DECIMAL(12, 2) DEFAULT 0
);

-- 2. Чай и Склад
CREATE TABLE Teas (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name TEXT NOT NULL,
    CurrentStock DECIMAL(12, 2) NOT NULL DEFAULT 0,
    Unit TEXT DEFAULT 'g' -- граммы
);

-- 3. История цен (Версионирование)
CREATE TABLE TeaPrices (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TeaId UUID REFERENCES Teas(Id),
    BrewPricePerGram DECIMAL(12, 2) NOT NULL, -- Цена заваривания
    SalePricePerGram DECIMAL(12, 2) NOT NULL, -- Цена продажи (магазин)
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 4. Визиты (Открытые счета)
CREATE TABLE Visits (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID REFERENCES Users(Id), -- Если NULL, то это аноним
    StartTime TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    EndTime TIMESTAMP WITH TIME ZONE,
    TotalAmount DECIMAL(12, 2) DEFAULT 0,
    IsClosed BOOLEAN DEFAULT FALSE
);

-- 5. Сессии заваривания
CREATE TABLE BrewingSessions (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TeaId UUID REFERENCES Teas(Id),
    PriceSnapshotId UUID REFERENCES TeaPrices(Id), -- Фиксация цены в момент заварки
    TotalGrams DECIMAL(12, 2) NOT NULL,
    TotalCost DECIMAL(12, 2) NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 6. Участники заварки (Связка Сессия <-> Визит)
CREATE TABLE BrewingParticipants (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    SessionId UUID REFERENCES BrewingSessions(Id) ON DELETE CASCADE,
    VisitId UUID REFERENCES Visits(Id),
    ShareCost DECIMAL(12, 2) NOT NULL -- Индивидуальная доля
);
