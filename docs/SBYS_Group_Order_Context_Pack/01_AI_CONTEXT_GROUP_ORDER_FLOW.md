# paybysharepay AI Context — Group Order Flow

## Flow
1. Host opretter gruppebetaling
2. Merchant vælges
3. Deltagere vælges fra venneliste
4. Host inkluderes automatisk som participant
5. Merchant er IKKE participant
6. participantToken genereres pr deltager
7. Personlige merchant-links genereres

## URL format
http://localhost:8081/?orderId={groupPaymentId}&merchantId={merchantId}&participantToken={participantToken}&api=http://localhost:5265

## Regler
- Merchant er ikke UserEntity
- groupPaymentId alene må aldrig give adgang
- Brug participantToken/joinToken
- Ordren må først gennemføres når alle deltagere er Authorized/Reserved/Paid
