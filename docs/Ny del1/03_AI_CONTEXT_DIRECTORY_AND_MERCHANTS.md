# SBYS AI Context — Directory and Merchants

## Mål
SBYS skal kunne vise både almindelige brugere og merchants/spisesteder i samme directory/find-liste.

## Vigtig regel
Merchant må ikke implementeres som en fake UserEntity.

Merchant skal have egen domain model.

## Directory
Directory er en fælles læsemodel til UI.

Den kan vise:
- User
- Merchant

Eksempel på fælles DTO:
- Type: User eller Merchant
- Id
- DisplayName
- Handle
- Subtitle
- LogoUrl

## MerchantEntity
Merchant bør indeholde visnings- og forretningsdata:

- Id
- DisplayName
- Handle
- Description
- LogoUrl
- WebsiteUrl
- GroupOrderUrl
- ContactEmail
- ContactPhone
- AddressLine1
- City
- PostalCode
- CountryCode
- IsActive
- CreatedAtUtc

## MerchantIntegrationEntity
Merchant integration bør indeholde tekniske data:

- MerchantId
- WebhookUrl
- DefaultReturnUrl
- AllowedOrigin
- ApiKeyHash
- SigningSecretHash
- IsEnabled
- UpdatedAtUtc

SBYS skal ikke gemme merchantens Stripe/Nets/QuickPay secrets i Model A.

## UI-regler
I directory-listen:
- Users vises som personer
- Merchants vises tydeligt som spisesteder
- Merchants kan have badge fx “Spisested”
- Users kan vælges som venner
- Merchants skal senere kunne følges/favoriseres

## Seed data
Til demo bør der findes mindst to merchants:
- Sticks & Sushi
- Gasoline Grill
