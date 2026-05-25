# PayNSync / PayBySharePay — MobilePay/Vipps Sandbox Copilot Guide

Disse filer er lavet til at blive uploadet én ad gangen til GitHub Copilot / Claude Sonnet 4.6 i Visual Studio.

## Anbefalet rækkefølge

1. `00-master-prompt.md`
2. `01-payment-provider-abstraction.md`
3. `02-domain-model-and-database.md`
4. `03-mobilepay-sandbox-provider.md`
5. `04-webhooks-and-status-sync.md`
6. `05-host-execute-capture-flow.md`
7. `06-test-dashboard-and-qa-plan.md`

## Vigtig præmis

Test aldrig ved at kopiere tokens eller betalings-id'er fra rigtige merchant-websites. Hele integrationen skal køre mod egen test-merchant/dummy merchant og MobilePay/Vipps Merchant Test miljø.

## Relevante officielle dokumentationspunkter

- Vipps MobilePay ePayment API quick start
- Merchant Test environment
- Payment lifecycle: Create → Reserve → Capture/Cancel → Refund
- Webhooks for payment status
- ePayment checklist

