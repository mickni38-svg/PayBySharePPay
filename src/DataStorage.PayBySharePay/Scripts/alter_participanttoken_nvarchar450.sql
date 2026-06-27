-- =============================================================
--  ALTER: OrderParticipants.ParticipantToken
--  NVARCHAR(MAX)  ->  NVARCHAR(450)
--
--  Kør dette på databaser oprettet INDEN fix i CREATE-scriptet.
--  SQL Server tillader ikke NVARCHAR(MAX) som index-nøglekolonne.
--
--  Trin:
--    1. Drop det unikke index (kræves for at ændre kolonnen)
--    2. Alter kolonnen til NVARCHAR(450)
--    3. Genopret det unikke index
-- =============================================================

USE paynsync_dk_db_test;   -- <-- skift databasenavn om nødvendigt
GO

-- 1. Drop eksisterende unikt index
IF EXISTS (
	SELECT 1 FROM sys.indexes
	WHERE name = 'IX_OrderParticipants_ParticipantToken'
	  AND object_id = OBJECT_ID('dbo.OrderParticipants')
)
BEGIN
	DROP INDEX IX_OrderParticipants_ParticipantToken
		ON dbo.OrderParticipants;
	PRINT 'Index IX_OrderParticipants_ParticipantToken droppet.';
END
GO

-- 2. Drop DEFAULT constraint på ParticipantToken (auto-navn fra SQL Server)
DECLARE @dfName NVARCHAR(256) = (
	SELECT dc.name
	FROM   sys.default_constraints dc
	JOIN   sys.columns             c  ON dc.parent_object_id = c.object_id
									 AND dc.parent_column_id = c.column_id
	WHERE  c.object_id  = OBJECT_ID('dbo.OrderParticipants')
	  AND  c.name       = 'ParticipantToken'
);
IF @dfName IS NOT NULL
BEGIN
	EXEC('ALTER TABLE dbo.OrderParticipants DROP CONSTRAINT ' + @dfName);
	PRINT 'DEFAULT constraint ' + @dfName + ' droppet.';
END
GO

-- 3. Ændr kolonnen
ALTER TABLE dbo.OrderParticipants
	ALTER COLUMN ParticipantToken NVARCHAR(450) NOT NULL;
PRINT 'Kolonne ParticipantToken ændret til NVARCHAR(450).';
GO

-- 4. Genopret det unikke index
CREATE UNIQUE INDEX IX_OrderParticipants_ParticipantToken
	ON dbo.OrderParticipants (ParticipantToken);
PRINT 'Unikt index IX_OrderParticipants_ParticipantToken genoprettet.';
GO

PRINT '=== ALTER fuldført ===';
GO
