-- =============================================================
--  PayBySharePay  --  Database: paynsync_dk_db_test
--  Genereret fra EF Core-entiteter og DbContext-konfiguration
--  Kan køres gentagne gange (idempotent via IF NOT EXISTS)
-- =============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'paynsync_dk_db_test')
BEGIN
	CREATE DATABASE paynsync_dk_db_test
		COLLATE Danish_Norwegian_CI_AS;
	PRINT 'Database paynsync_dk_db_test oprettet.';
END
GO

USE paynsync_dk_db_test;
GO

-- =============================================================
--  1. Participants
--     ParticipantType: Person = 0, Merchant = 1
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Participants' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.Participants (
		Id                  INT             NOT NULL IDENTITY(1,1),
		[Type]              INT             NOT NULL,           -- 0 = Person, 1 = Merchant
		[Name]              NVARCHAR(MAX)   NOT NULL,
		Email               NVARCHAR(MAX)   NULL,
		Phone               NVARCHAR(MAX)   NULL,
		PasswordHash        NVARCHAR(MAX)   NULL,
		-- Merchant-specifikke felter
		CompanyName         NVARCHAR(MAX)   NULL,
		CvrNumber           NVARCHAR(MAX)   NULL,
		VatNumber           NVARCHAR(MAX)   NULL,
		ContactPerson       NVARCHAR(MAX)   NULL,
		ContactEmail        NVARCHAR(MAX)   NULL,
		ContactPhone        NVARCHAR(MAX)   NULL,
		CompanyAddress      NVARCHAR(MAX)   NULL,
		PaymentReference    NVARCHAR(MAX)   NULL,
		PayoutAccountInfo   NVARCHAR(MAX)   NULL,
		PaymentProvider     NVARCHAR(MAX)   NULL,
		GroupOrderUrl       NVARCHAR(MAX)   NULL,
		CONSTRAINT PK_Participants PRIMARY KEY (Id)
	);
	PRINT 'Tabel Participants oprettet.';
END
GO

-- =============================================================
--  2. Orders
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.Orders (
		Id                      INT             NOT NULL IDENTITY(1,1),
		CreatedByParticipantId  INT             NOT NULL,
		Title                   NVARCHAR(MAX)   NOT NULL,
		Category                NVARCHAR(MAX)   NULL,
		[Message]               NVARCHAR(MAX)   NULL,
		[Status]                NVARCHAR(MAX)   NOT NULL DEFAULT 'Collecting',
		MerchantParticipantId   INT             NULL,
		JoinToken               NVARCHAR(MAX)   NULL,
		CreatedAt               DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
		CONSTRAINT PK_Orders PRIMARY KEY (Id),
		CONSTRAINT FK_Orders_Participants_CreatedBy
			FOREIGN KEY (CreatedByParticipantId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION,
		CONSTRAINT FK_Orders_Participants_Merchant
			FOREIGN KEY (MerchantParticipantId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION
	);
	CREATE INDEX IX_Orders_CreatedByParticipantId
		ON dbo.Orders (CreatedByParticipantId);
	CREATE INDEX IX_Orders_MerchantParticipantId
		ON dbo.Orders (MerchantParticipantId);
	PRINT 'Tabel Orders oprettet.';
END
GO

-- =============================================================
--  3. FriendRelations
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FriendRelations' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.FriendRelations (
		Id          INT         NOT NULL IDENTITY(1,1),
		InitiatorId INT         NOT NULL,
		ReceiverId  INT         NOT NULL,
		CreatedAt   DATETIME2   NOT NULL DEFAULT SYSUTCDATETIME(),
		CONSTRAINT PK_FriendRelations PRIMARY KEY (Id),
		CONSTRAINT FK_FriendRelations_Participants_Initiator
			FOREIGN KEY (InitiatorId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION,
		CONSTRAINT FK_FriendRelations_Participants_Receiver
			FOREIGN KEY (ReceiverId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION
	);
	CREATE INDEX IX_FriendRelations_InitiatorId
		ON dbo.FriendRelations (InitiatorId);
	CREATE INDEX IX_FriendRelations_ReceiverId
		ON dbo.FriendRelations (ReceiverId);
	PRINT 'Tabel FriendRelations oprettet.';
END
GO

-- =============================================================
--  4. OrderParticipants
--     Status: 'Pending' | 'Invited' | 'Paid' | 'OrderSubmitted'
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderParticipants' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.OrderParticipants (
		Id               INT             NOT NULL IDENTITY(1,1),
		OrderId          INT             NOT NULL,
		ParticipantId    INT             NOT NULL,
		[Status]         NVARCHAR(MAX)   NOT NULL DEFAULT 'Pending',
		ParticipantToken NVARCHAR(450)   NOT NULL DEFAULT NEWID(),
		CONSTRAINT PK_OrderParticipants PRIMARY KEY (Id),
		CONSTRAINT FK_OrderParticipants_Orders
			FOREIGN KEY (OrderId)
			REFERENCES dbo.Orders(Id)
			ON DELETE CASCADE,
		CONSTRAINT FK_OrderParticipants_Participants
			FOREIGN KEY (ParticipantId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION
	);
	-- EF kræver unik index på ParticipantToken
	CREATE UNIQUE INDEX IX_OrderParticipants_ParticipantToken
		ON dbo.OrderParticipants (ParticipantToken);
	CREATE INDEX IX_OrderParticipants_OrderId
		ON dbo.OrderParticipants (OrderId);
	CREATE INDEX IX_OrderParticipants_ParticipantId
		ON dbo.OrderParticipants (ParticipantId);
	PRINT 'Tabel OrderParticipants oprettet.';
END
GO

-- =============================================================
--  5. Payments  (simpel betalingspost pr. deltager)
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Payments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.Payments (
		Id            INT             NOT NULL IDENTITY(1,1),
		OrderId       INT             NOT NULL,
		ParticipantId INT             NOT NULL,
		Amount        DECIMAL(18, 2)  NOT NULL,
		[Status]      NVARCHAR(MAX)   NOT NULL DEFAULT 'Pending',
		CreatedAt     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
		CONSTRAINT PK_Payments PRIMARY KEY (Id),
		CONSTRAINT FK_Payments_Orders
			FOREIGN KEY (OrderId)
			REFERENCES dbo.Orders(Id)
			ON DELETE CASCADE,
		CONSTRAINT FK_Payments_Participants
			FOREIGN KEY (ParticipantId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION
	);
	CREATE INDEX IX_Payments_OrderId
		ON dbo.Payments (OrderId);
	CREATE INDEX IX_Payments_ParticipantId
		ON dbo.Payments (ParticipantId);
	PRINT 'Tabel Payments oprettet.';
END
GO

-- =============================================================
--  6. Messages
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Messages' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.Messages (
		Id            INT             NOT NULL IDENTITY(1,1),
		OrderId       INT             NOT NULL,
		ParticipantId INT             NOT NULL,
		Content       NVARCHAR(MAX)   NOT NULL,
		CreatedAt     DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
		IsRead        BIT             NOT NULL DEFAULT 0,
		CONSTRAINT PK_Messages PRIMARY KEY (Id),
		CONSTRAINT FK_Messages_Orders
			FOREIGN KEY (OrderId)
			REFERENCES dbo.Orders(Id)
			ON DELETE CASCADE,
		CONSTRAINT FK_Messages_Participants
			FOREIGN KEY (ParticipantId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION
	);
	CREATE INDEX IX_Messages_OrderId
		ON dbo.Messages (OrderId);
	CREATE INDEX IX_Messages_ParticipantId
		ON dbo.Messages (ParticipantId);
	PRINT 'Tabel Messages oprettet.';
END
GO

-- =============================================================
--  7. MerchantOrderDrafts
--     PaymentMode: 'AuthorizeThenCapture' | 'ManualCapture'
--     Status:      'Draft' | 'Collecting' | 'AllAuthorized' | 'Released' | 'Expired'
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MerchantOrderDrafts' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.MerchantOrderDrafts (
		Id                      INT             NOT NULL IDENTITY(1,1),
		OrderId                 INT             NOT NULL,
		MerchantParticipantId   INT             NOT NULL,
		MerchantDraftReference  NVARCHAR(MAX)   NOT NULL,
		SubtotalAmount          DECIMAL(18, 2)  NOT NULL,
		TotalAmount             DECIMAL(18, 2)  NOT NULL,
		Currency                NVARCHAR(MAX)   NOT NULL DEFAULT 'DKK',
		ParticipantId           INT             NULL,
		PaymentMode             NVARCHAR(MAX)   NOT NULL DEFAULT 'AuthorizeThenCapture',
		[Status]                NVARCHAR(MAX)   NOT NULL DEFAULT 'Draft',
		ExpiresAtUtc            DATETIME2       NULL,
		CreatedAtUtc            DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
		CONSTRAINT PK_MerchantOrderDrafts PRIMARY KEY (Id),
		CONSTRAINT FK_MerchantOrderDrafts_Orders
			FOREIGN KEY (OrderId)
			REFERENCES dbo.Orders(Id)
			ON DELETE CASCADE,
		CONSTRAINT FK_MerchantOrderDrafts_Participants_Merchant
			FOREIGN KEY (MerchantParticipantId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION,
		CONSTRAINT FK_MerchantOrderDrafts_Participants_Participant
			FOREIGN KEY (ParticipantId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION
	);
	CREATE INDEX IX_MerchantOrderDrafts_OrderId
		ON dbo.MerchantOrderDrafts (OrderId);
	CREATE INDEX IX_MerchantOrderDrafts_MerchantParticipantId
		ON dbo.MerchantOrderDrafts (MerchantParticipantId);
	CREATE INDEX IX_MerchantOrderDrafts_ParticipantId
		ON dbo.MerchantOrderDrafts (ParticipantId);
	PRINT 'Tabel MerchantOrderDrafts oprettet.';
END
GO

-- =============================================================
--  8. MerchantOrderLines
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MerchantOrderLines' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.MerchantOrderLines (
		Id                    INT             NOT NULL IDENTITY(1,1),
		MerchantOrderDraftId  INT             NOT NULL,
		ParticipantId         INT             NULL,
		LineId                NVARCHAR(MAX)   NOT NULL,
		[Name]                NVARCHAR(MAX)   NOT NULL,
		Quantity              INT             NOT NULL,
		UnitPrice             DECIMAL(18, 2)  NOT NULL,
		LineTotal             DECIMAL(18, 2)  NOT NULL,
		CONSTRAINT PK_MerchantOrderLines PRIMARY KEY (Id),
		CONSTRAINT FK_MerchantOrderLines_MerchantOrderDrafts
			FOREIGN KEY (MerchantOrderDraftId)
			REFERENCES dbo.MerchantOrderDrafts(Id)
			ON DELETE CASCADE,
		CONSTRAINT FK_MerchantOrderLines_Participants
			FOREIGN KEY (ParticipantId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION
	);
	CREATE INDEX IX_MerchantOrderLines_MerchantOrderDraftId
		ON dbo.MerchantOrderLines (MerchantOrderDraftId);
	CREATE INDEX IX_MerchantOrderLines_ParticipantId
		ON dbo.MerchantOrderLines (ParticipantId);
	PRINT 'Tabel MerchantOrderLines oprettet.';
END
GO

-- =============================================================
--  9. ParticipantPayments
--     Status (int):
--       0  Created              10 ReservationStarted
--      20  Reserved             30 ReservationFailed
--      40  CapturePending       50 Captured
--      60  CaptureFailed        70 Cancelled
--      80  Expired              90 Refunded
--     RowVersion = SQL rowversion (automatisk optimistisk låsning)
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ParticipantPayments' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.ParticipantPayments (
		Id                          INT             NOT NULL IDENTITY(1,1),
		OrderId                     INT             NOT NULL,
		ParticipantId               INT             NOT NULL,
		MerchantId                  NVARCHAR(MAX)   NULL,
		AmountMinorUnits            BIGINT          NOT NULL,
		Currency                    NVARCHAR(MAX)   NOT NULL DEFAULT 'DKK',
		[Status]                    INT             NOT NULL DEFAULT 0,
		ProviderName                NVARCHAR(MAX)   NULL,
		ProviderPaymentId           NVARCHAR(MAX)   NULL,
		ProviderReference           NVARCHAR(MAX)   NULL,
		ReservationStartedAtUtc     DATETIME2       NULL,
		ReservedAtUtc               DATETIME2       NULL,
		CaptureStartedAtUtc         DATETIME2       NULL,
		CapturedAtUtc               DATETIME2       NULL,
		CancelledAtUtc              DATETIME2       NULL,
		LastErrorCode               NVARCHAR(MAX)   NULL,
		LastErrorMessage            NVARCHAR(MAX)   NULL,
		CreatedAtUtc                DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
		RowVersion                  ROWVERSION      NOT NULL,
		CONSTRAINT PK_ParticipantPayments PRIMARY KEY (Id),
		CONSTRAINT FK_ParticipantPayments_Orders
			FOREIGN KEY (OrderId)
			REFERENCES dbo.Orders(Id)
			ON DELETE NO ACTION,
		CONSTRAINT FK_ParticipantPayments_Participants
			FOREIGN KEY (ParticipantId)
			REFERENCES dbo.Participants(Id)
			ON DELETE NO ACTION
	);
	CREATE INDEX IX_ParticipantPayments_OrderId
		ON dbo.ParticipantPayments (OrderId);
	CREATE INDEX IX_ParticipantPayments_ParticipantId
		ON dbo.ParticipantPayments (ParticipantId);
	PRINT 'Tabel ParticipantPayments oprettet.';
END
GO

-- =============================================================
-- 10. PaymentEventLogs  (audit-trail for betalingshændelser)
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PaymentEventLogs' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.PaymentEventLogs (
		Id                    INT             NOT NULL IDENTITY(1,1),
		OrderId               INT             NOT NULL,
		ParticipantPaymentId  INT             NOT NULL,
		ProviderPaymentId     NVARCHAR(MAX)   NULL,
		EventType             NVARCHAR(100)   NOT NULL,
		OldStatus             INT             NULL,
		NewStatus             INT             NULL,
		PayloadJson           NVARCHAR(MAX)   NULL,
		CorrelationId         NVARCHAR(100)   NULL,
		CreatedAtUtc          DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
		CONSTRAINT PK_PaymentEventLogs PRIMARY KEY (Id)
	);
	CREATE INDEX IX_PaymentEventLogs_ParticipantPaymentId
		ON dbo.PaymentEventLogs (ParticipantPaymentId);
	CREATE INDEX IX_PaymentEventLogs_OrderId
		ON dbo.PaymentEventLogs (OrderId);
	PRINT 'Tabel PaymentEventLogs oprettet.';
END
GO

-- =============================================================
-- 11. __EFMigrationsHistory
--     Forhindrer EF Core i at køre migrationer igen
-- =============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = '__EFMigrationsHistory' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.[__EFMigrationsHistory] (
		MigrationId     NVARCHAR(150)   NOT NULL,
		ProductVersion  NVARCHAR(32)    NOT NULL,
		CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
	);
	PRINT 'Tabel __EFMigrationsHistory oprettet.';
END
GO

-- Registrér alle kendte migrationer så EF Core ikke prøver at køre dem
IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE MigrationId = '20260505153849_InitialCreate')
	INSERT INTO dbo.[__EFMigrationsHistory] VALUES ('20260505153849_InitialCreate', '9.0.16');
IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE MigrationId = '20260506184515_AddOrderCreatedBy')
	INSERT INTO dbo.[__EFMigrationsHistory] VALUES ('20260506184515_AddOrderCreatedBy', '9.0.16');
IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE MigrationId = '20260506185436_AddMerchantOrderDraft')
	INSERT INTO dbo.[__EFMigrationsHistory] VALUES ('20260506185436_AddMerchantOrderDraft', '9.0.16');
IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE MigrationId = '20260508204647_AddMerchantToOrder')
	INSERT INTO dbo.[__EFMigrationsHistory] VALUES ('20260508204647_AddMerchantToOrder', '9.0.16');
IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE MigrationId = '20260515161555_AddParticipantToOrderLine')
	INSERT INTO dbo.[__EFMigrationsHistory] VALUES ('20260515161555_AddParticipantToOrderLine', '9.0.16');
IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE MigrationId = '20260516143930_AddParticipantTokenAndDraftParticipant')
	INSERT INTO dbo.[__EFMigrationsHistory] VALUES ('20260516143930_AddParticipantTokenAndDraftParticipant', '9.0.16');
IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE MigrationId = '20260516151034_AddMessageIsRead')
	INSERT INTO dbo.[__EFMigrationsHistory] VALUES ('20260516151034_AddMessageIsRead', '9.0.16');
IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE MigrationId = '20260524060213_AddParticipantPasswordHash')
	INSERT INTO dbo.[__EFMigrationsHistory] VALUES ('20260524060213_AddParticipantPasswordHash', '9.0.16');
IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE MigrationId = '20260525082640_AddParticipantPaymentAndEventLog')
	INSERT INTO dbo.[__EFMigrationsHistory] VALUES ('20260525082640_AddParticipantPaymentAndEventLog', '9.0.16');
GO

PRINT '=== paynsync_dk_db_test: alle tabeller klar ===';
GO
