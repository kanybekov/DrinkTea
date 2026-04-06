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

-- 7. Финансовые транзакции (История платежей)
CREATE TABLE Transactions (
	Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	VisitId UUID REFERENCES Visits(Id), -- К какому визиту относится (может быть NULL для пополнений)
	UserId UUID REFERENCES Users(Id),   -- Кто платил (для постоянщиков)
	Amount DECIMAL(12, 2) NOT NULL,     -- Сумма
	PaymentMethod TEXT NOT NULL,        -- Internal, Cash, Card
	CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 8. Розничные продажи (Магазин)
CREATE TABLE Sales (
	Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	TeaId UUID NOT NULL REFERENCES Teas(Id),        -- Какой чай купили
	UserId UUID REFERENCES Users(Id),               -- Кто купил (NULL для анонимов)
	Grams DECIMAL(12, 2) NOT NULL,                 -- Вес покупки
	TotalCost DECIMAL(12, 2) NOT NULL,             -- Итоговая сумма (Grams * SalePricePerGram)
	PaymentMethod TEXT NOT NULL,                   -- Internal, Cash, Card
	CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 	Ремарка: 
-- 		Таблица отделена от заварок, так как в магазине 
-- 		используется другой прайс и нет понятия "делимого счета".

-- Меняем тип колонки в транзакциях
ALTER TABLE Transactions 
ALTER COLUMN PaymentMethod TYPE INTEGER 
USING (CASE 
    WHEN PaymentMethod = 'Internal' THEN 1 
    WHEN PaymentMethod = 'Cash' THEN 2 
    WHEN PaymentMethod = 'Card' THEN 3 
    ELSE 2 END);

-- То же самое для таблицы Sales
ALTER TABLE Sales 
ALTER COLUMN PaymentMethod TYPE INTEGER 
USING (CASE 
    WHEN PaymentMethod = 'Internal' THEN 1 
    WHEN PaymentMethod = 'Cash' THEN 2 
    WHEN PaymentMethod = 'Card' THEN 3 
    ELSE 2 END);


-- Добавляем универсальное поле для заметок мастера
ALTER TABLE Visits ADD COLUMN Note TEXT;

-- 	Ремарка: 
-- 		В это поле мастер пишет идентификатор (номер стола, примету), 
-- 		чтобы различать гостей на экране Dashboard.


-- Добавляем колонку логина (уникальную) и пароля
ALTER TABLE Users ADD COLUMN IF NOT EXISTS Login TEXT UNIQUE;
ALTER TABLE Users ADD COLUMN IF NOT EXISTS PasswordHash TEXT;
ALTER TABLE Users ADD COLUMN IF NOT EXISTS Role TEXT DEFAULT 'Customer';

-- Пример: создаем Мастера
INSERT INTO Users (FullName, Login, PasswordHash, Role)
VALUES ('Главный Мастер', 'admin', '$2a$11$ev7.S.n8J8J0X/3C7v9GueZfI5rA5R8f5Y5F/I5z1mQyP9K7X8k2G', 'Admin')
ON CONFLICT (Login) DO NOTHING;
-- Пароль выше: admin123


-- Преобразуем колонку Role в числа
ALTER TABLE Users 
ALTER COLUMN Role TYPE INTEGER 
USING (CASE 
    WHEN Role = 'Admin' OR Role = 'Master' THEN 1 
    ELSE 0 END);

-- Устанавливаем значение по умолчанию 0 (Customer)
ALTER TABLE Users ALTER COLUMN Role SET DEFAULT 0;


UPDATE Users SET PasswordHash = '$2a$11$sjiT6lscbU5D3QV4orAUAOLo5DvA2fLI2IqOxbFzDlT6f7zY1YQBq' WHERE Login = 'admin';


-- Добавляем ответственного за заварку
ALTER TABLE BrewingSessions ADD COLUMN StaffId UUID REFERENCES Users(Id);

-- Добавляем ответственного за продажу
ALTER TABLE Sales ADD COLUMN StaffId UUID REFERENCES Users(Id);

-- Добавляем ответственного за транзакцию (кто принял деньги)
ALTER TABLE Transactions ADD COLUMN StaffId UUID REFERENCES Users(Id);

-- 	Ремарка: 
-- 		Теперь мы всегда сможем построить отчет: "Сколько чая заварил мастер Алексей".

ALTER TABLE Transactions ADD COLUMN IF NOT EXISTS StaffId UUID REFERENCES Users(Id);
ALTER TABLE Transactions ADD COLUMN IF NOT EXISTS Description TEXT;

ALTER TABLE BrewingSessions 
ADD COLUMN IsFinished BOOLEAN NOT NULL DEFAULT FALSE;

-----------------------------------------














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

