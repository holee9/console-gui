using System.Runtime.CompilerServices;

// Allow the test assembly to access internal types (SignatureVerifier, BackupManager, UpdateChecker).
// IEC 62304: internal components must be testable to achieve the required 85%+ branch coverage.
[assembly: InternalsVisibleTo("HnVue.Update.Tests")]
