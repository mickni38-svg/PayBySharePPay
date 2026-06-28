-- ============================================================
-- Seed: Test-merchants til PayNSync (sandbox)
--
-- Begge spisesteder bruger samme GroupOrderUrl i TEST fordi
-- https://merchant.paynsync.dk kun er én dummy-side.
-- I produktion skal hvert salgssted have sin EGEN URL.
--
-- MSN-numrene er Vipps MobilePay sandbox Merchant Serial Numbers.
-- ============================================================

DECLARE @TestMerchantUrl NVARCHAR(500) = 'https://merchant.paynsync.dk';

-- --------------------------------------------------------
-- 1. Pizzeria Roma
--    MSN: 2067756
-- --------------------------------------------------------
IF NOT EXISTS (
	SELECT 1 FROM dbo.Participants
	WHERE [Type] = 1 AND Name = 'Pizzeria Roma'
)
BEGIN
	INSERT INTO dbo.Participants
	(
		[Type], Name, Email, CompanyName, CvrNumber,
		ContactEmail, CompanyAddress, GroupOrderUrl,
		VippsMerchantSerialNumber, PaymentReference, PaymentProvider
	)
	VALUES
	(
		1,                              -- Type = Merchant
		'Pizzeria Roma',
		'hej@pizzeriaroma.dk',
		'Pizzeria Roma ApS',
		'34109855',
		'hej@pizzeriaroma.dk',
		'Vesterbrogade 12, 1620 København V',
		@TestMerchantUrl,
		'2067756',                      -- MSN: Vipps sandbox MSN for Pizzeria Roma
		'ROMA-PAY',
		'MobilePay'
	);
	PRINT 'Oprettede Pizzeria Roma';
END
ELSE
BEGIN
	UPDATE dbo.Participants
	SET VippsMerchantSerialNumber = '2067756',
		GroupOrderUrl             = @TestMerchantUrl,
		PaymentProvider           = 'MobilePay'
	WHERE [Type] = 1 AND Name = 'Pizzeria Roma';
	PRINT 'Opdaterede Pizzeria Roma (MSN + URL)';
END

-- --------------------------------------------------------
-- 2. sticks & sushi
--    MSN: 2067757
-- --------------------------------------------------------
IF NOT EXISTS (
	SELECT 1 FROM dbo.Participants
	WHERE [Type] = 1 AND Name = 'sticks & sushi'
)
BEGIN
	INSERT INTO dbo.Participants
	(
		[Type], Name, Email, CompanyName, CvrNumber,
		ContactEmail, CompanyAddress, GroupOrderUrl,
		VippsMerchantSerialNumber, PaymentReference, PaymentProvider
	)
	VALUES
	(
		1,                              -- Type = Merchant
		'sticks & sushi',
		'info@sticksandsushi.dk',
		'sticks & sushi ApS',
		NULL,
		'info@sticksandsushi.dk',
		NULL,
		@TestMerchantUrl,
		'2067757',                      -- MSN: Vipps sandbox MSN for sticks & sushi
		'SUSHI-PAY',
		'MobilePay'
	);
	PRINT 'Oprettede sticks & sushi';
END
ELSE
BEGIN
	UPDATE dbo.Participants
	SET VippsMerchantSerialNumber = '2067757',
		GroupOrderUrl             = @TestMerchantUrl,
		PaymentProvider           = 'MobilePay'
	WHERE [Type] = 1 AND Name = 'sticks & sushi';
	PRINT 'Opdaterede sticks & sushi (MSN + URL)';
END

-- Vis resultatet
SELECT Id, Name, CompanyName, VippsMerchantSerialNumber, GroupOrderUrl, PaymentProvider
FROM dbo.Participants
WHERE [Type] = 1 AND Name IN ('Pizzeria Roma', 'sticks & sushi');
