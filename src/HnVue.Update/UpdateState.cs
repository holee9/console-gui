namespace HnVue.Update;

/// <summary>
/// Represents the current state of a software update operation.
/// Used for atomic rollback and state tracking during the update lifecycle.
/// </summary>
public enum UpdateState
{
    /// <summary>
    /// Update is currently in progress.
    /// Set when ApplyUpdateAsync begins execution.
    /// </summary>
    InProgress,

    /// <summary>
    /// Update package has been successfully staged.
    /// Backup has been created, files are ready for installation on restart.
    /// </summary>
    Staged,

    /// <summary>
    /// Update has been successfully completed.
    /// All verification checks passed and package is staged.
    /// </summary>
    Completed,

    /// <summary>
    /// Update operation failed.
    /// System has rolled back to previous state.
    /// </summary>
    Failed,

    /// <summary>
    /// Update was rolled back after a failure.
    /// Previous application state has been restored from backup.
    /// </summary>
    RolledBack
}
