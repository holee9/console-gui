using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

// @MX:ANCHOR IDetectorInterface - @MX:REASON: FPD hardware abstraction for image acquisition, safety-critical HAZ-RAD interlock
/// <summary>
/// Abstracts flat-panel detector (FPD) hardware communication.
/// Implemented by <c>OwnDetectorAdapter</c> (production, 자사 SDK) and
/// <c>DetectorSimulator</c> (development/test).
/// Third-party vendor adapters are placed in <c>HnVue.Detector/ThirdParty/</c>.
/// </summary>
/// <remarks>
/// Implements SWR-WF-030 (detector arming), SWR-WF-031 (image acquisition),
/// SWR-WF-032 (abort command). Referenced by HAZ-RAD (radiation safety interlock).
/// IEC 62304 §5.3.6 traceability: WF-3xx module.
///
/// Typical acquisition sequence:
/// <code>
///   ConnectAsync()              // establish link, load calibration
///   ArmAsync(Sync)              // arm for hardware X-ray trigger
///   // X-ray fires → detector senses radiation → readout begins automatically
///   // ImageAcquired event fires when readout is complete
///   DisconnectAsync()           // clean shutdown
/// </code>
/// </remarks>
public interface IDetectorInterface
{
    /// <summary>Gets the current detector hardware state.</summary>
    DetectorState CurrentState { get; }

    /// <summary>Occurs when the detector state changes.</summary>
    event EventHandler<DetectorStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Occurs when a full image frame has been read out and is ready for processing.
    /// Subscribers receive a <see cref="ImageAcquiredEventArgs"/> containing raw pixel data.
    /// </summary>
    event EventHandler<ImageAcquiredEventArgs>? ImageAcquired;

    /// <summary>
    /// Establishes communication with the detector and loads calibration data.
    /// </summary>
    /// <remarks>SWR-WF-030: CONNECT command, loads gain/offset calibration into panel.</remarks>
    Task<Result> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from the detector and releases hardware resources.
    /// </summary>
    Task<Result> DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Arms the detector for acquisition.
    /// In <see cref="DetectorTriggerMode.Sync"/> mode the detector waits for the X-ray hardware signal.
    /// In <see cref="DetectorTriggerMode.FreeRun"/> mode it acquires immediately on call (test only).
    /// </summary>
    /// <param name="triggerMode">Acquisition trigger mode.</param>
    /// <param name="cancellationToken">Token to cancel the arm command.</param>
    /// <remarks>SWR-WF-031: ARM command. Must be called before each exposure.</remarks>
    Task<Result> ArmAsync(
        DetectorTriggerMode triggerMode = DetectorTriggerMode.Sync,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aborts the current acquisition immediately. Must be callable from any thread.
    /// </summary>
    /// <remarks>SWR-WF-032: ABORT command. HAZ-RAD safety interlock — must not fail silently.</remarks>
    Task<Result> AbortAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries current detector status (temperature, ready state, firmware version).
    /// </summary>
    Task<Result<DetectorStatus>> GetStatusAsync(CancellationToken cancellationToken = default);
}
