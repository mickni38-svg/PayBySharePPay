-- ============================================================
-- Seed: Test-merchants til PayNSync (sandbox)
--
-- Udfyld variablerne herunder med nøgler fra Vipps portalen:
-- portal.vippsmobilepay.com → Udvikler → Test → Vis nøgler
-- ============================================================

-- ── Fælles ───────────────────────────────────────────────────
DECLARE @TestMerchantUrl NVARCHAR(500) = 'https://merchant.paynsync.dk';

-- ── Pizza Roma (MSN: 2067760 – Reserve Capture) ──────────────
DECLARE @PizzaMsn             NVARCHAR(50)  = '2067760';
DECLARE @PizzaClientId        NVARCHAR(200) = '0d72310d-1c48-4502-8b6e-0dd4484abe2e';
DECLARE @PizzaClientSecret    NVARCHAR(500) = 'ROz8Q~xoyqK5Wxmy5.JbQWoucZ1N_JnND8Fs3cmS';
DECLARE @PizzaSubscriptionKey NVARCHAR(100) = '627b0e639117336871336b7e24bb03dd';

-- ── Sticks (MSN: 2067761 – Reserve Capture) ──────────────────
DECLARE @SticksMsn             NVARCHAR(50)  = '2067761';
DECLARE @SticksClientId        NVARCHAR(200) = 'f41c3394-cd7e-4819-9079-cde584483826';
DECLARE @SticksClientSecret    NVARCHAR(500) = 'l0E8Q~wsZLNbSBiTQbCBFAshTocPgIIa1w14Ya41';
DECLARE @SticksSubscriptionKey NVARCHAR(100) = 'f4bece59598c0d02ff9e00f54242a279';

-- =============================================================

-- ── 1. Pizza Roma ─────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Participants WHERE [Type] = 1 AND Name = 'Pizza Roma')
BEGIN
    INSERT INTO dbo.Participants ([Type], Name, Email, CompanyName, CvrNumber, ContactEmail, CompanyAddress, GroupOrderUrl, VippsMerchantSerialNumber, VippsClientId, VippsClientSecret, VippsSubscriptionKey, PaymentReference, PaymentProvider)
    VALUES (1, 'Pizza Roma', 'hej@pizzaroma.dk', 'Pizza Roma ApS', '34109855', 'hej@pizzaroma.dk', 'Vesterbrogade 12, 1620 Koebenhavn V', @TestMerchantUrl, @PizzaMsn, @PizzaClientId, @PizzaClientSecret, @PizzaSubscriptionKey, 'PIZZA-PAY', 'MobilePay');
    PRINT 'Oprettede Pizza Roma';
END
ELSE
BEGIN
    UPDATE dbo.Participants SET VippsMerchantSerialNumber = @PizzaMsn, VippsClientId = @PizzaClientId, VippsClientSecret = @PizzaClientSecret, VippsSubscriptionKey = @PizzaSubscriptionKey, GroupOrderUrl = @TestMerchantUrl, PaymentProvider = 'MobilePay' WHERE [Type] = 1 AND Name = 'Pizza Roma';
    PRINT 'Opdaterede Pizza Roma';
END

-- ── 2. Sticks ─────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Participants WHERE [Type] = 1 AND Name = 'Sticks')
BEGIN
    INSERT INTO dbo.Participants ([Type], Name, Email, CompanyName, CvrNumber, ContactEmail, CompanyAddress, GroupOrderUrl, VippsMerchantSerialNumber, VippsClientId, VippsClientSecret, VippsSubscriptionKey, PaymentReference, PaymentProvider)
    VALUES (1, 'Sticks', 'info@sticks.dk', 'Sticks ApS', NULL, 'info@sticks.dk', NULL, @TestMerchantUrl, @SticksMsn, @SticksClientId, @SticksClientSecret, @SticksSubscriptionKey, 'STICKS-PAY', 'MobilePay');
    PRINT 'Oprettede Sticks';
END
ELSE
BEGIN
    UPDATE dbo.Participants SET VippsMerchantSerialNumber = @SticksMsn, VippsClientId = @SticksClientId, VippsClientSecret = @SticksClientSecret, VippsSubscriptionKey = @SticksSubscriptionKey, GroupOrderUrl = @TestMerchantUrl, PaymentProvider = 'MobilePay' WHERE [Type] = 1 AND Name = 'Sticks';
    PRINT 'Opdaterede Sticks';
END

-- ── Resultat ──────────────────────────────────────────────────
SELECT Id, Name, VippsMerchantSerialNumber,
    CASE WHEN LEN(ISNULL(VippsClientId,''))        > 0 THEN 'JA' ELSE 'MANGLER' END AS ClientId,
    CASE WHEN LEN(ISNULL(VippsClientSecret,''))    > 0 THEN 'JA' ELSE 'MANGLER' END AS ClientSecret,
    CASE WHEN LEN(ISNULL(VippsSubscriptionKey,'')) > 0 THEN 'JA' ELSE 'MANGLER' END AS SubscriptionKey,
    GroupOrderUrl
FROM dbo.Participants
WHERE [Type] = 1 AND Name IN ('Pizza Roma', 'Sticks');
