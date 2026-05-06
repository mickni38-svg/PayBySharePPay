# SBYS AI Context — Group Payment Flow

## Formål
En bruger skal kunne oprette en gruppebetaling i SBYS og invitere venner.

## MVP-flow

1. User logger ind eller bruger midlertidig test-user
2. User finder venner i directory
3. User opretter gruppebetaling
4. User vælger deltagere fra vennelisten
5. GroupPayment gemmes i databasen
6. GroupPaymentMembers gemmes i databasen
7. Overview viser gruppebetaling og status

## GroupPayment
Bør indeholde:
- Id
- CreatedByUserId
- Title
- Message
- IconKey
- IsActive
- CreatedAtUtc
- Status

Mulige statusser:
- Draft
- Collecting
- WaitingForParticipants
- WaitingForPayment
- Ready
- Completed
- Cancelled
- Expired

## GroupPaymentMember
Bør indeholde:
- Id
- GroupPaymentId
- UserId
- Status
- CreatedAtUtc
- AcceptedAtUtc
- PaidAtUtc/AuthorizedAtUtc

Mulige member-statusser:
- Invited
- Accepted
- Rejected
- Authorized
- Paid
- Cancelled
- Expired

## Vigtig regel
Ordren må først frigives til merchant/køkken, når alle relevante deltagere har reserveret/betalt.
