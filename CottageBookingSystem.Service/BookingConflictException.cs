namespace CottageBookingSystem.Service;

public sealed class BookingConflictException(Exception innerException)
    : Exception(ConflictMessage, innerException)
{
    public const string ConflictMessage = "הצימר כבר נתפס על ידי משתמש אחר לתאריכים אלו. אנא בחר תאריכים אחרים";
}
