# PayNSync – Known Flows

Describes the major user and system flows as they exist in the current codebase.

---

## 1. Create Group Order

**Actor:** Host (Person participant, logged in)  
**Entry point:** `POST /api/orders`

```
Host fills in order form (title, category, optional message)
  → Selects a Merchant from directory search
  → Selects Friend participants
  → Submits CreateOrderRequest

API: CreateOrderRequest → CreateOrderDto → OrderService.CreateOrderAsync()

OrderService:
  1. Validates title or category present
  2. Loads creator participant
  3. Loads merchant participant (if provided)
  4. Generates JoinToken (GUID) for the order
  5. Creates Order entity (status = "Collecting")
  6. Adds creator as OrderParticipant (status = "Accepted", unique ParticipantToken)
  7. Adds each invited participant as OrderParticipant (status = "Invited", unique ParticipantToken)
  8. Saves order

  If merchant assigned:
	For each OrderParticipant (including host):
	  - Builds link: {merchant.GroupOrderUrl}?orderId={id}&merchantId={mid}&participantToken={token}
	  - Creates Message record for that participant with the link
  Else:
	  - Creates invitation Message for each invited participant

Response: OrderDto (id, status, title, ...)
```

---

## 2. Select Merchant

**Actor:** Host, during or after order creation  
**Entry point:** `GET /api/directory/search?query=...` or `GET /api/participants/search?query=...`

```
Host types merchant name in search field
  → GET /api/directory/search?query={text}
  → Returns DirectoryEntryDto[] (type = Merchant)
  → Host selects one merchant
  → merchantParticipantId is included in CreateOrderRequest
```

Merchant metadata shown in UI: `CompanyName`, `CompanyAddress` (from `Participant` fields).

---

## 3. Add Participants

**Actor:** Host, during order creation  
**Entry point:** `GET /api/friends/{participantId}` + `GET /api/directory/search`

```
Host sees friend list (GET /api/friends/{id})
  → Selects friends to include as participantIds[] in CreateOrderRequest
  → Non-friend participants can be found via directory search

Note: There is no endpoint to add participants AFTER an order is created.
```

---

## 4. Participant Orders via Merchant Link

**Actor:** Participant (follows link from Message)  
**Entry point:** Merchant Demo (Pizzeria Roma) at `https://merchant.paynsync.dk`

```
Participant opens message in app
  → Sees personalised merchant link
  → Opens link: {merchantUrl}?orderId={id}&merchantId={mid}&participantToken={token}

Merchant Demo (Frontend.MerchantDemo/index.html):
  1. Reads orderId, merchantId, participantToken from URL query params
  2. Shows restaurant menu (Pizzeria Roma hardcoded items)
  3. User ticks items, total is computed client-side
  4. Clicks "Gruppebetaling" button

  submitOrder():
	POST {API_BASE}/api/merchant-orders
	body: {
	  orderId, merchantParticipantId, participantToken,
	  merchantDraftReference: "ROMA-{timestamp}",
	  subtotalAmount, totalAmount, currency: "DKK",
	  paymentMode: "GroupPay",
	  expiresAtUtc: now + 24h,
	  lines: [{ lineId, name, quantity, unitPrice, lineTotal }]
	}

API: MerchantOrdersController.InitOrder() → MerchantOrderService.InitOrderAsync()

MerchantOrderService:
  1. Loads order + merchant
  2. Validates merchant type = Merchant
  3. Validates participantToken → finds OrderParticipant
  4. Rejects if participant type = Merchant
  5. Deletes any existing draft for this participant+order
  6. Creates MerchantOrderDraft + MerchantOrderLine records
  7. Sets OrderParticipant.Status = "OrderSubmitted"
  8. Calls GroupPaymentOrchestrationService.ReserveParticipantPaymentAsync()
	 → Creates ParticipantPayment, calls IPaymentProvider.ReserveAsync()

  Fake provider (RedirectUrl == null — reserved synchronously):
	9. MerchantOrderService calls CheckAndSetReadyToPayByReservedAsync(orderId) directly
	   → If ALL non-merchant participants have ParticipantPayment.Status = Reserved:
		 → Set Order.Status = "ReadyToPay"
		 → Send Message to host: "Alle har bestilt og reserveret betaling..."

  Vipps provider (RedirectUrl != null):
	9. Returns redirect URL to client
	   → Merchant Demo redirects participant to MobilePay app
	   → Participant swipes/approves in MobilePay
	   → Vipps sends webhook → VippsCallbackController (AUTHORIZED/RESERVE)
		 → SetReservedAsync
		 → CheckAndSetReadyToPayByReservedAsync(orderId)
		   → If ALL non-merchant participants Reserved → Order.Status = "ReadyToPay"
```

---

## 5. Host Approves (Capture Flow)

**Actor:** Host (logged in)  
**Entry point:** `POST /api/orders/{id}/approve`

```
Host sees order status = "ReadyToPay" in frontend
  → Sees each participant's payment reservation status
  → Clicks "Godkend og betal" button

API: OrdersController.ApproveOrder() → GroupPaymentOrchestrationService.ApproveAndCaptureAllAsync()

Orchestration:
  1. Validates host (requestingParticipantId == order.CreatedByParticipantId)
  2. Validates order status in [ReadyToPay, HostApproved, Capturing, PartiallyFailed]
  3. Loads Reserved payments
  4. Validates at least 1 Reserved payment exists
  5. Order.Status → "HostApproved"  [saved]
  6. All Reserved payments → "CapturePending"
  7. Order.Status → "Capturing"  [saved]
  8. For each CapturePending payment:
	 a. Build CapturePaymentRequest (ProviderPaymentId, amount, currency, idempotencyKey)
	 b. Call IPaymentProvider.CaptureAsync()
	 c. Success → SetCapturedAsync (ParticipantPayment.Status = Captured)
	 d. Failure → SetCaptureFailedAsync + Order.Status = "PartiallyFailed" + break
  9. If all captured: Order.Status = "Paid"
 10. Sends MerchantCallback (HTTP POST to Merchant.GroupOrderUrl)

Response: ApproveAndCaptureResult { allCaptured, orderStatus, results[] }
Frontend: shows success/failure per participant
```

---

## 6. Participant Reserves Payment (Before Approval)

**Actor:** Participant (logged in)  
**Entry point:** `POST /api/orders/{id}/reserve`

> Note: The frontend code path that calls this endpoint is not fully traced. The endpoint exists and is wired up.

```
Participant (or host on behalf) calls:
  POST /api/orders/{id}/reserve
  body: { participantId, merchantId, amountMinorUnits, currency, returnUrl, callbackUrl }

Orchestration:
  1. Idempotency check: existing non-failed payment? → return existing
  2. Create ParticipantPayment (status = Created)
  3. Set temp ProviderPaymentId, status = ReservationStarted
  4. Call IPaymentProvider.ReserveAsync()
	 - Fake: returns success + fake redirect URL immediately
	 - Real (Vipps): returns redirect URL, user must complete in MobilePay app
  5. Update ProviderPaymentId with real provider ID
  6. If provider returned "Reserved"/"Authorized": status → Reserved

Response: { success, participantPaymentId, redirectUrl }
Real provider: redirectUrl → user sent to MobilePay/Vipps to authorize
Fake provider: redirectUrl is https://fake-payment.local/pay/...

Webhook callback (real provider):
  POST /api/payments/vipps/callbacks/{reference}
  → Sets status to Reserved, Captured, or Cancelled based on Vipps event name
```

---

## 7. Pending Payments / Reminders

**Actor:** Host (frontend view)  
**Entry point:** `GET /api/orders?participantId={id}` + client-side computation

```
Frontend loads all orders for current user
  → Filters to host's own orders (createdByParticipantId == currentUserId)
  → Calls computePendingSummary() client-side:
	For each order:
	  → counts participants with status "Invited" (not yet ordered)
  → Displays PendingParticipantsComponent with count badges

No backend endpoint specifically for pending participants.
No automated reminder sending — host can manually send messages.
```

---

## 8. Cancel Order

**Actor:** Host (logged in)  
**Entry point:** `POST /api/orders/{id}/cancel`

```
Host sees order in non-Paid state
  → Clicks cancel

Orchestration:
  1. Validates host
  2. Rejects if already Paid
  3. Idempotent: if already Cancelled, returns success
  4. For each ParticipantPayment:
	 - Skip if Captured, Cancelled, or Expired
	 - If no ProviderPaymentId: set Cancelled directly
	 - Else: call IPaymentProvider.CancelAsync()
	   - Success: SetCancelledAsync
	   - Failure: collect error but continue
  5. Order.Status = "Cancelled" (always, even if some cancels failed)

Response: { success, orderStatus, cancelledCount, skippedCount, errors[] }
```

---

## 9. Merchant Link Flow (Full End-to-End)

```
[Host creates order with merchant]
  │
  ├─► Merchant invitation link generated per participant
  │   Format: {merchant.GroupOrderUrl}?orderId=X&merchantId=Y&participantToken=Z
  │
  └─► Message stored in DB for each participant

[Participant opens message in app]
  │
  └─► Clicks link → opens Merchant Demo in browser

[Merchant Demo]
  │
  ├─► Shows menu items (hardcoded for Pizzeria Roma)
  ├─► Participant selects items
  └─► Clicks "Gruppebetaling"
		│
		└─► POST /api/merchant-orders (anonymous)
			  │
			  ├─► Validates token
			  ├─► Creates MerchantOrderDraft + Lines
			  ├─► Sets OrderParticipant.Status = "OrderSubmitted"
			  ├─► Starts payment reservation (ReserveParticipantPaymentAsync)
			  │   ├─► Fake: reserved synchronously → CheckAndSetReadyToPayByReservedAsync
			  │   └─► Vipps: redirect to MobilePay → participant approves → webhook → CheckAndSetReadyToPayByReservedAsync
			  └─► If ALL non-merchant participants now have Reserved payment → Order.Status = "ReadyToPay"
					│
					└─► Message sent to host

[Host receives ReadyToPay notification]
  │
  └─► Opens order in app → sees "Klar til betaling"
		│
		└─► All participant payments are Reserved — host can approve and capture
			  │
			  └─► Clicks "Godkend og betal"
					│
					└─► POST /api/orders/{id}/approve
						  │
						  ├─► Captures all reserved payments
						  └─► Sends merchant callback (paid notification)
```

---

## 10. Webhook / Provider Callback Flow

**For Vipps MobilePay (real provider):**

```
User completes payment in MobilePay app
  │
  └─► Vipps sends POST to:
	  /api/payments/vipps/callbacks/{participantPaymentId}
		│
		├─► Lookup by reference (ProviderPaymentId)
		├─► Map Vipps event name:
		│   AUTHORIZED / RESERVE → SetReservedAsync
		│                          + CheckAndSetReadyToPayByReservedAsync
		│                            (→ Order.Status = "ReadyToPay" if all participants Reserved)
		│   CAPTURED             → Logger og ignorerer (ingen state-ændring — capture sker via /approve-flow)
		│   CANCELLED / ABORTED  → SetCancelledAsync
		│   EXPIRED / TERMINATED → SetReservationFailedAsync (sætter ReservationFailed, ikke Expired)
		│   Andet                → Logger og ignorerer
		└─► Returns 200 (always — so Vipps doesn't retry on not-found)
```

**For Fake provider:**  
Fake provider sets status synchronously during `ReserveAsync` — no webhook needed.  
Generic webhook endpoints (`/webhooks/provider`, `/webhooks/mobilepay`) exist for testing.

---

## 11. Google Login Flow *(NYT)*

**Actor:** Person (ny eller eksisterende Google-bruger)  
**Entry point:** `POST /api/auth/google-login`

```
Klient (Angular frontend) sender Google ID-token (fra Google Sign-In)
  → POST /api/auth/google-login
  → AuthController.GoogleLogin(ExternalLoginRequest)
  → ExternalAuthService.GoogleLoginAsync(idToken)

ExternalAuthService:
  1. Læser Google:ClientId fra konfiguration (krævet)
  2. GoogleJsonWebSignature.ValidateAsync(idToken)
     → Returnerer payload: Subject (userId), Email, Name
  3. Slår op i ParticipantExternalLogins:
     Provider = "Google" + ProviderUserId = payload.Subject
     → Fundet: hent eksisterende Participant → returner JWT
  4. Tjekker om payload.Email allerede er registreret med PasswordHash
     → Ja: kast ExternalLoginEmailConflictException → HTTP 409
       (ingen automatisk sammenslåning af konto)
  5. Opretter ny Participant (Type = Person, Name = displayName, Email = email)
  6. Opretter ny ParticipantExternalLogin (Provider="Google", ProviderUserId, Email)
  7. Returnerer ParticipantDto

AuthController: returnerer JWT + ParticipantId + Name + ExpiresAt (= now + 480 min)
```

---

## 12. Vipps Test-User Mapping Flow *(NYT)*

**Actor:** Deltager (i sandbox/dev-miljø)  
**Entry point:** `GET /api/participants/vipps-test-users` + `PATCH /api/participants/{id}/vipps-test-user`

```
Deltager åbner Vipps test-user valg i frontend (sandbox-miljø):
  → GET /api/participants/vipps-test-users
  → ParticipantsController.GetVippsTestUsers()
  → ParticipantService.GetVippsTestPersonsAsync()
  ← VippsTestPersonDto[] med Id, Name, Phone, MappedByParticipantId
     (MappedByParticipantId = null → ledig testperson)

Deltager vælger en ledig testperson:
  → PATCH /api/participants/{id}/vipps-test-user
     body: { vippsTestUserId: 5 }
  → ParticipantsController.SetVippsTestUser()
  → ParticipantService.SetVippsTestUserAsync(participantId, vippsTestUserId)
  ← 204 No Content

Herefter bruges testpersonens telefonnummer når PayNSync starter
en Vipps MobilePay-reservation for deltageren i sandbox.

Note: VippsTestUserId-feltet er kun relevant i sandbox.
Feltet bruges ikke i produktionsflowet.
```
