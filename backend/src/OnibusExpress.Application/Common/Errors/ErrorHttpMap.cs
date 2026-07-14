namespace OnibusExpress.Application.Common.Errors;

/// <summary>
/// Single source of truth for translating <c>ErrorCode</c> -&gt; HTTP status,
/// consumed by the global exception handler in the API (Phase 3).
/// </summary>
public static class ErrorHttpMap
{
    public static int ToStatusCode(string errorCode) => errorCode switch
    {
        "DOCUMENT_INVALID" or "EMAIL_INVALID" or "RESERVATION_CODE_INVALID"
            or "SEAT_INVALID" or "SEAT_OUT_OF_RANGE" => 400,
        "NOT_FOUND" => 404,
        "SEAT_TAKEN" or "TRIP_ALREADY_DEPARTED"
            or "CANCELLATION_TOO_LATE" or "RESERVATION_ALREADY_CANCELLED"
            or "RESERVATION_CODE_DUPLICATE" => 409,
        "COULD_NOT_GENERATE_CODE" => 503,
        _ => 500,
    };
}
