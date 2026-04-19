using HnVue.Common.Results;

namespace HnVue.Common.ErrorHandling;

/// <summary>
/// Classifies errors into categories for safe-state transition decisions.
/// Implements WP-T1-ERR error handling matrix.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>Transient error — safe to retry.</summary>
    Transient,

    /// <summary>Recoverable error — operation can continue with degraded functionality.</summary>
    Recoverable,

    /// <summary>Critical error — requires safe-state transition (halt operation).</summary>
    Critical,

    /// <summary>Fatal error — requires application restart or administrator intervention.</summary>
    Fatal,
}

/// <summary>
/// Categorizes error codes into severity levels for the error handling matrix.
/// </summary>
public static class ErrorCategory
{
    /// <summary>
    /// Classifies an error code into its severity level.
    /// </summary>
    public static ErrorSeverity Classify(ErrorCode errorCode) => errorCode switch
    {
        // Transient: network/communication issues, retry-safe
        ErrorCode.NetworkTimeout => ErrorSeverity.Transient,
        ErrorCode.CommunicationFailure => ErrorSeverity.Transient,
        ErrorCode.HardwareNoResponse => ErrorSeverity.Transient,
        ErrorCode.ConnectionRefused => ErrorSeverity.Transient,
        ErrorCode.SslHandshakeFailed => ErrorSeverity.Transient,
        ErrorCode.DatabaseError => ErrorSeverity.Transient,
        ErrorCode.DicomConnectionFailed => ErrorSeverity.Transient,
        ErrorCode.DicomStoreFailed => ErrorSeverity.Transient,
        ErrorCode.DicomQueryFailed => ErrorSeverity.Transient,
        ErrorCode.DicomPrintFailed => ErrorSeverity.Transient,

        // Recoverable: validation/logic errors, user can retry with different input
        ErrorCode.ValidationFailed => ErrorSeverity.Recoverable,
        ErrorCode.AlreadyExists => ErrorSeverity.Recoverable,
        ErrorCode.NotFound => ErrorSeverity.Recoverable,
        ErrorCode.AuthenticationFailed => ErrorSeverity.Recoverable,
        ErrorCode.PasswordPolicyViolation => ErrorSeverity.Recoverable,
        ErrorCode.PinNotSet => ErrorSeverity.Recoverable,
        ErrorCode.TokenExpired => ErrorSeverity.Recoverable,
        ErrorCode.TokenRevoked => ErrorSeverity.Recoverable,
        ErrorCode.InsufficientPermission => ErrorSeverity.Recoverable,
        ErrorCode.RateLimitExceeded => ErrorSeverity.Recoverable,
        ErrorCode.ReauthenticationRequired => ErrorSeverity.Recoverable,

        // Critical: safety-related, requires safe-state transition
        ErrorCode.DoseLimitExceeded => ErrorSeverity.Critical,
        ErrorCode.DoseInterlock => ErrorSeverity.Critical,
        ErrorCode.ExposureAborted => ErrorSeverity.Critical,
        ErrorCode.InvalidStateTransition => ErrorSeverity.Critical,
        ErrorCode.GeneratorNotReady => ErrorSeverity.Critical,
        ErrorCode.DetectorNotReady => ErrorSeverity.Critical,
        ErrorCode.EncryptionFailed => ErrorSeverity.Critical,
        ErrorCode.AuditTamperingDetected => ErrorSeverity.Critical,
        ErrorCode.RoleElevationBlocked => ErrorSeverity.Critical,
        ErrorCode.CalibrationDataMissing => ErrorSeverity.Critical,

        // Fatal: requires admin intervention
        ErrorCode.MigrationFailed => ErrorSeverity.Fatal,
        ErrorCode.SignatureVerificationFailed => ErrorSeverity.Fatal,
        ErrorCode.UpdatePackageCorrupt => ErrorSeverity.Fatal,
        ErrorCode.RollbackFailed => ErrorSeverity.Fatal,
        ErrorCode.TlsConnectionFailed => ErrorSeverity.Fatal,
        ErrorCode.AccountLocked => ErrorSeverity.Fatal,
        ErrorCode.TokenInvalid => ErrorSeverity.Fatal,

        // Default: treat unknowns as recoverable
        _ => ErrorSeverity.Recoverable,
    };

    /// <summary>
    /// Determines whether a safe-state transition is required for the given error.
    /// </summary>
    public static bool RequiresSafeStateTransition(ErrorCode errorCode)
        => Classify(errorCode) is ErrorSeverity.Critical or ErrorSeverity.Fatal;

    /// <summary>
    /// Determines whether the operation is safe to retry for the given error.
    /// </summary>
    public static bool IsRetryable(ErrorCode errorCode)
        => Classify(errorCode) == ErrorSeverity.Transient;
}
