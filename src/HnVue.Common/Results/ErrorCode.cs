namespace HnVue.Common.Results;

// @MX:ANCHOR ErrorCode enum - @MX:REASON: IEC 62304 safety classification mapping, 270+ consumers system-wide
/// <summary>
/// Standard error codes used across all HnVue modules, grouped by domain.
/// IEC 62304 traceability: each range maps to a functional domain.
/// </summary>
public enum ErrorCode
{
    // ── General (0xxx) ─────────────────────────────────────────────────────────

    /// <summary>Unclassified or unknown error.</summary>
    Unknown = 0,

    /// <summary>Input or business-rule validation failed.</summary>
    ValidationFailed = 1000,

    /// <summary>Requested resource was not found.</summary>
    NotFound = 1001,

    /// <summary>Resource already exists and cannot be duplicated.</summary>
    AlreadyExists = 1002,

    /// <summary>Operation was cancelled by the caller.</summary>
    OperationCancelled = 1003,

    // ── Security (2xxx) ────────────────────────────────────────────────────────

    /// <summary>Credentials were invalid or not provided.</summary>
    AuthenticationFailed = 2000,

    /// <summary>Account has been locked due to policy enforcement.</summary>
    AccountLocked = 2001,

    /// <summary>Authentication token has expired.</summary>
    TokenExpired = 2002,

    /// <summary>Authentication token is malformed or tampered.</summary>
    TokenInvalid = 2003,

    /// <summary>User does not have the required role or permission.</summary>
    InsufficientPermission = 2004,

    /// <summary>New password does not meet complexity requirements.</summary>
    PasswordPolicyViolation = 2005,

    /// <summary>Quick PIN has not been set for the user account.</summary>
    PinNotSet = 2006,

    /// <summary>Authentication token has been revoked via JTI denylist. SWR-CS-077.</summary>
    TokenRevoked = 2007,

    /// <summary>Calibration data (Gain/Offset) is missing or expired. SWR-IP-039.</summary>
    CalibrationDataMissing = 2008,

    // ── Data (3xxx) ────────────────────────────────────────────────────────────

    /// <summary>A database-level error occurred.</summary>
    DatabaseError = 3000,

    /// <summary>Database migration script failed to apply.</summary>
    MigrationFailed = 3001,

    /// <summary>Data encryption or decryption operation failed.</summary>
    EncryptionFailed = 3002,

    // ── Workflow (4xxx) ────────────────────────────────────────────────────────

    /// <summary>Requested state transition is not allowed from the current state.</summary>
    InvalidStateTransition = 4000,

    /// <summary>X-ray generator is not ready to accept commands.</summary>
    GeneratorNotReady = 4001,

    /// <summary>Flat-panel detector is not ready to acquire images.</summary>
    DetectorNotReady = 4002,

    /// <summary>Exposure sequence was aborted before completion.</summary>
    ExposureAborted = 4003,

    /// <summary>Calculated dose exceeds the configured safety limit.</summary>
    DoseLimitExceeded = 4004,

    /// <summary>Exposure blocked or aborted by dose interlock (SWR-WF-023~025).</summary>
    DoseInterlock = 4005,

    // ── DICOM (5xxx) ───────────────────────────────────────────────────────────

    /// <summary>Network connection to a DICOM peer failed.</summary>
    DicomConnectionFailed = 5000,

    /// <summary>C-STORE operation to PACS failed.</summary>
    DicomStoreFailed = 5001,

    /// <summary>C-FIND worklist query failed.</summary>
    DicomQueryFailed = 5002,

    /// <summary>DICOM print operation to printer AE failed.</summary>
    DicomPrintFailed = 5003,

    // ── Incident (6xxx) ────────────────────────────────────────────────────────

    /// <summary>Writing an incident log entry failed.</summary>
    IncidentLogFailed = 6000,

    // ── Update (7xxx) ──────────────────────────────────────────────────────────

    /// <summary>Software package digital-signature verification failed.</summary>
    SignatureVerificationFailed = 7000,

    /// <summary>Update package is corrupted or incomplete.</summary>
    UpdatePackageCorrupt = 7001,

    /// <summary>Software rollback to previous version failed.</summary>
    RollbackFailed = 7002,

    // ── CD Burning (8xxx) ──────────────────────────────────────────────────────

    /// <summary>Disc burn operation failed.</summary>
    BurnFailed = 8000,

    /// <summary>Post-burn disc verification failed.</summary>
    DiscVerificationFailed = 8001,

    // ── Imaging (9xxx) ─────────────────────────────────────────────────────────

    /// <summary>Image processing pipeline failed.</summary>
    ImageProcessingFailed = 9000,

    /// <summary>Image file format is not supported by the processor.</summary>
    UnsupportedImageFormat = 9001,
}
