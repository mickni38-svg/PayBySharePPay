namespace DataStorage.PayBySharePay.Entities;

public enum ParticipantPaymentStatus
{
    Created = 0,
    ReservationStarted = 10,
    Reserved = 20,
    ReservationFailed = 30,
    CapturePending = 40,
    Captured = 50,
    CaptureFailed = 60,
    Cancelled = 70,
    Expired = 80,
    Refunded = 90
}
