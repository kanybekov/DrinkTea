CREATE TABLE IF NOT EXISTS TeaPublicReviews (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TeaId UUID NOT NULL REFERENCES Teas(Id) ON DELETE CASCADE,
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Rating INTEGER NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment TEXT NOT NULL DEFAULT '',
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_tea_public_reviews_teaid_createdat
    ON TeaPublicReviews(TeaId, CreatedAt DESC);

WITH ranked_public AS (
    SELECT id,
           ROW_NUMBER() OVER (PARTITION BY teaid, userid ORDER BY createdat DESC, id DESC) AS rn
    FROM teapublicreviews
)
DELETE FROM teapublicreviews
WHERE id IN (SELECT id FROM ranked_public WHERE rn > 1);

CREATE UNIQUE INDEX IF NOT EXISTS uq_tea_public_reviews_tea_user
    ON TeaPublicReviews(TeaId, UserId);

CREATE TABLE IF NOT EXISTS TeaPrivateNotes (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TeaId UUID NOT NULL REFERENCES Teas(Id) ON DELETE CASCADE,
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    NoteText TEXT NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_tea_private_notes_user_tea_createdat
    ON TeaPrivateNotes(UserId, TeaId, CreatedAt DESC);

WITH ranked_private AS (
    SELECT id,
           ROW_NUMBER() OVER (PARTITION BY teaid, userid ORDER BY createdat DESC, id DESC) AS rn
    FROM teaprivatenotes
)
DELETE FROM teaprivatenotes
WHERE id IN (SELECT id FROM ranked_private WHERE rn > 1);

CREATE UNIQUE INDEX IF NOT EXISTS uq_tea_private_notes_tea_user
    ON TeaPrivateNotes(TeaId, UserId);
