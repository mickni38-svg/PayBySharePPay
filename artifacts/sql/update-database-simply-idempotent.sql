IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NULL,
        [Message] nvarchar(max) NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [Participants] (
        [Id] int NOT NULL IDENTITY,
        [Type] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [CompanyName] nvarchar(max) NULL,
        [CvrNumber] nvarchar(max) NULL,
        [VatNumber] nvarchar(max) NULL,
        [ContactPerson] nvarchar(max) NULL,
        [ContactEmail] nvarchar(max) NULL,
        [ContactPhone] nvarchar(max) NULL,
        [CompanyAddress] nvarchar(max) NULL,
        [PaymentReference] nvarchar(max) NULL,
        [PayoutAccountInfo] nvarchar(max) NULL,
        [PaymentProvider] nvarchar(max) NULL,
        CONSTRAINT [PK_Participants] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [FriendRelations] (
        [Id] int NOT NULL IDENTITY,
        [InitiatorId] int NOT NULL,
        [ReceiverId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FriendRelations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FriendRelations_Participants_InitiatorId] FOREIGN KEY ([InitiatorId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FriendRelations_Participants_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [Messages] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ParticipantId] int NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Messages_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Messages_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderParticipants] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ParticipantId] int NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_OrderParticipants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderParticipants_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderParticipants_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ParticipantId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Payments_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FriendRelations_InitiatorId] ON [FriendRelations] ([InitiatorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FriendRelations_ReceiverId] ON [FriendRelations] ([ReceiverId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_OrderId] ON [Messages] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_ParticipantId] ON [Messages] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderParticipants_OrderId] ON [OrderParticipants] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderParticipants_ParticipantId] ON [OrderParticipants] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_ParticipantId] ON [Payments] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260505153849_InitialCreate', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506184515_AddOrderCreatedBy'
)
BEGIN
    ALTER TABLE [Orders] ADD [CreatedByParticipantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506184515_AddOrderCreatedBy'
)
BEGIN
    CREATE INDEX [IX_Orders_CreatedByParticipantId] ON [Orders] ([CreatedByParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506184515_AddOrderCreatedBy'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Participants_CreatedByParticipantId] FOREIGN KEY ([CreatedByParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506184515_AddOrderCreatedBy'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506184515_AddOrderCreatedBy', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE TABLE [MerchantOrderDrafts] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [MerchantParticipantId] int NOT NULL,
        [MerchantDraftReference] nvarchar(max) NOT NULL,
        [SubtotalAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [PaymentMode] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [ExpiresAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_MerchantOrderDrafts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MerchantOrderDrafts_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MerchantOrderDrafts_Participants_MerchantParticipantId] FOREIGN KEY ([MerchantParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE TABLE [MerchantOrderLines] (
        [Id] int NOT NULL IDENTITY,
        [MerchantOrderDraftId] int NOT NULL,
        [LineId] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_MerchantOrderLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MerchantOrderLines_MerchantOrderDrafts_MerchantOrderDraftId] FOREIGN KEY ([MerchantOrderDraftId]) REFERENCES [MerchantOrderDrafts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE INDEX [IX_MerchantOrderDrafts_MerchantParticipantId] ON [MerchantOrderDrafts] ([MerchantParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE INDEX [IX_MerchantOrderDrafts_OrderId] ON [MerchantOrderDrafts] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE INDEX [IX_MerchantOrderLines_MerchantOrderDraftId] ON [MerchantOrderLines] ([MerchantOrderDraftId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506185436_AddMerchantOrderDraft', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    ALTER TABLE [Participants] ADD [GroupOrderUrl] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    ALTER TABLE [Orders] ADD [JoinToken] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    ALTER TABLE [Orders] ADD [MerchantParticipantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    CREATE INDEX [IX_Orders_MerchantParticipantId] ON [Orders] ([MerchantParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Participants_MerchantParticipantId] FOREIGN KEY ([MerchantParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260508204647_AddMerchantToOrder', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515161555_AddParticipantToOrderLine'
)
BEGIN
    ALTER TABLE [MerchantOrderLines] ADD [ParticipantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515161555_AddParticipantToOrderLine'
)
BEGIN
    CREATE INDEX [IX_MerchantOrderLines_ParticipantId] ON [MerchantOrderLines] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515161555_AddParticipantToOrderLine'
)
BEGIN
    ALTER TABLE [MerchantOrderLines] ADD CONSTRAINT [FK_MerchantOrderLines_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515161555_AddParticipantToOrderLine'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260515161555_AddParticipantToOrderLine', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516143930_AddParticipantTokenAndDraftParticipant'
)
BEGIN
    ALTER TABLE [OrderParticipants] ADD [ParticipantToken] nvarchar(450) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516143930_AddParticipantTokenAndDraftParticipant'
)
BEGIN
    ALTER TABLE [MerchantOrderDrafts] ADD [ParticipantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516143930_AddParticipantTokenAndDraftParticipant'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderParticipants_ParticipantToken] ON [OrderParticipants] ([ParticipantToken]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516143930_AddParticipantTokenAndDraftParticipant'
)
BEGIN
    CREATE INDEX [IX_MerchantOrderDrafts_ParticipantId] ON [MerchantOrderDrafts] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516143930_AddParticipantTokenAndDraftParticipant'
)
BEGIN
    ALTER TABLE [MerchantOrderDrafts] ADD CONSTRAINT [FK_MerchantOrderDrafts_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516143930_AddParticipantTokenAndDraftParticipant'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260516143930_AddParticipantTokenAndDraftParticipant', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516151034_AddMessageIsRead'
)
BEGIN
    ALTER TABLE [Messages] ADD [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260516151034_AddMessageIsRead'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260516151034_AddMessageIsRead', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260524060213_AddParticipantPasswordHash'
)
BEGIN
    ALTER TABLE [Participants] ADD [PasswordHash] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260524060213_AddParticipantPasswordHash'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260524060213_AddParticipantPasswordHash', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525082640_AddParticipantPaymentAndEventLog'
)
BEGIN
    CREATE TABLE [ParticipantPayments] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ParticipantId] int NOT NULL,
        [MerchantId] nvarchar(max) NULL,
        [AmountMinorUnits] bigint NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [ProviderName] nvarchar(max) NULL,
        [ProviderPaymentId] nvarchar(max) NULL,
        [ProviderReference] nvarchar(max) NULL,
        [ReservationStartedAtUtc] datetime2 NULL,
        [ReservedAtUtc] datetime2 NULL,
        [CaptureStartedAtUtc] datetime2 NULL,
        [CapturedAtUtc] datetime2 NULL,
        [CancelledAtUtc] datetime2 NULL,
        [LastErrorCode] nvarchar(max) NULL,
        [LastErrorMessage] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ParticipantPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ParticipantPayments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ParticipantPayments_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525082640_AddParticipantPaymentAndEventLog'
)
BEGIN
    CREATE TABLE [PaymentEventLogs] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ParticipantPaymentId] int NOT NULL,
        [ProviderPaymentId] nvarchar(max) NULL,
        [EventType] nvarchar(100) NOT NULL,
        [OldStatus] int NULL,
        [NewStatus] int NULL,
        [PayloadJson] nvarchar(max) NULL,
        [CorrelationId] nvarchar(100) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_PaymentEventLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525082640_AddParticipantPaymentAndEventLog'
)
BEGIN
    CREATE INDEX [IX_ParticipantPayments_OrderId] ON [ParticipantPayments] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525082640_AddParticipantPaymentAndEventLog'
)
BEGIN
    CREATE INDEX [IX_ParticipantPayments_ParticipantId] ON [ParticipantPayments] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525082640_AddParticipantPaymentAndEventLog'
)
BEGIN
    CREATE INDEX [IX_PaymentEventLogs_OrderId] ON [PaymentEventLogs] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525082640_AddParticipantPaymentAndEventLog'
)
BEGIN
    CREATE INDEX [IX_PaymentEventLogs_ParticipantPaymentId] ON [PaymentEventLogs] ([ParticipantPaymentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260525082640_AddParticipantPaymentAndEventLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260525082640_AddParticipantPaymentAndEventLog', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628134839_AddRawMerchantPayloadJson'
)
BEGIN
    ALTER TABLE [MerchantOrderDrafts] ADD [RawMerchantPayloadJson] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628134839_AddRawMerchantPayloadJson'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260628134839_AddRawMerchantPayloadJson', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628175137_AddVippsMerchantSerialNumber'
)
BEGIN
    ALTER TABLE [Participants] ADD [VippsMerchantSerialNumber] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628175137_AddVippsMerchantSerialNumber'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260628175137_AddVippsMerchantSerialNumber', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628193159_AddMerchantVippsCredentials'
)
BEGIN
    ALTER TABLE [Participants] ADD [VippsClientId] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628193159_AddMerchantVippsCredentials'
)
BEGIN
    ALTER TABLE [Participants] ADD [VippsClientSecret] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628193159_AddMerchantVippsCredentials'
)
BEGIN
    ALTER TABLE [Participants] ADD [VippsSubscriptionKey] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260628193159_AddMerchantVippsCredentials'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260628193159_AddMerchantVippsCredentials', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703193037_AddParticipantExternalLogin'
)
BEGIN
    CREATE TABLE [ParticipantExternalLogins] (
        [Id] int NOT NULL IDENTITY,
        [ParticipantId] int NOT NULL,
        [Provider] nvarchar(50) NOT NULL,
        [ProviderUserId] nvarchar(256) NOT NULL,
        [Email] nvarchar(256) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_ParticipantExternalLogins] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ParticipantExternalLogins_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703193037_AddParticipantExternalLogin'
)
BEGIN
    CREATE INDEX [IX_ParticipantExternalLogins_ParticipantId] ON [ParticipantExternalLogins] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703193037_AddParticipantExternalLogin'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ParticipantExternalLogins_Provider_ProviderUserId] ON [ParticipantExternalLogins] ([Provider], [ProviderUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703193037_AddParticipantExternalLogin'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703193037_AddParticipantExternalLogin', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703201732_AddVippsTestUserId'
)
BEGIN
    ALTER TABLE [Participants] ADD [VippsTestUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703201732_AddVippsTestUserId'
)
BEGIN
    CREATE INDEX [IX_Participants_VippsTestUserId] ON [Participants] ([VippsTestUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703201732_AddVippsTestUserId'
)
BEGIN
    ALTER TABLE [Participants] ADD CONSTRAINT [FK_Participants_Participants_VippsTestUserId] FOREIGN KEY ([VippsTestUserId]) REFERENCES [Participants] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260703201732_AddVippsTestUserId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260703201732_AddVippsTestUserId', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815173756_AddMerchantLogo'
)
BEGIN
    ALTER TABLE [Participants] ADD [LogoContentType] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815173756_AddMerchantLogo'
)
BEGIN
    ALTER TABLE [Participants] ADD [LogoFileName] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815173756_AddMerchantLogo'
)
BEGIN
    ALTER TABLE [Participants] ADD [LogoImageData] varbinary(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815173756_AddMerchantLogo'
)
BEGIN
    ALTER TABLE [Participants] ADD [LogoUpdatedAtUtc] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260815173756_AddMerchantLogo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260815173756_AddMerchantLogo', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903093600_AddParticipantDeliveryAddress'
)
BEGIN
    ALTER TABLE [Participants] ADD [Address] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903093600_AddParticipantDeliveryAddress'
)
BEGIN
    ALTER TABLE [Participants] ADD [PostalCode] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903093600_AddParticipantDeliveryAddress'
)
BEGIN
    ALTER TABLE [Participants] ADD [City] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903093600_AddParticipantDeliveryAddress'
)
BEGIN
    ALTER TABLE [Participants] ADD [Country] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903093600_AddParticipantDeliveryAddress'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903093600_AddParticipantDeliveryAddress', N'9.0.19');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903095000_AddOrderDeliveryAddressSnapshot'
)
BEGIN
    ALTER TABLE [Orders] ADD [DeliveryAddress] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903095000_AddOrderDeliveryAddressSnapshot'
)
BEGIN
    ALTER TABLE [Orders] ADD [DeliveryPostalCode] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903095000_AddOrderDeliveryAddressSnapshot'
)
BEGIN
    ALTER TABLE [Orders] ADD [DeliveryCity] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903095000_AddOrderDeliveryAddressSnapshot'
)
BEGIN
    ALTER TABLE [Orders] ADD [DeliveryCountry] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903095000_AddOrderDeliveryAddressSnapshot'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903095000_AddOrderDeliveryAddressSnapshot', N'9.0.19');
END;

COMMIT;
GO

