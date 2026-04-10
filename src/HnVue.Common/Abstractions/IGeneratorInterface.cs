using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

// @MX:ANCHOR IGeneratorInterface - @MX:REASON: X-ray generator hardware abstraction, safety-critical exposure control, HAZ-RAD interlock
/// <summary>
/// Abstracts X-ray generator hardware communication.
/// Implemented by GeneratorSerialPort (production RS-232) and GeneratorSimulator (development/test).
/// </summary>
/// <remarks>
/// Implements SWR-WF-020 (generator control), SWR-WF-021 (exposure command),
/// SWR-WF-022 (abort command). Referenced by HAZ-RAD (radiation safety interlock).
/// IEC 62304 §5.3.6 traceability: WF-2xx module.
/// </remarks>
public interface IGeneratorInterface
{
    /// <summary>Gets the current generator hardware state.</summary>
    GeneratorState CurrentState { get; }

    /// <summary>Occurs when the generator state changes.</summary>
    event EventHandler<GeneratorStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Establishes communication with the generator.
    /// </summary>
    Task<Result> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from the generator.
    /// </summary>
    Task<Result> DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Programs exposure parameters (kVp, mAs, body part) into the generator.
    /// Must be called before <see cref="TriggerExposureAsync"/>.
    /// </summary>
    /// <remarks>SWR-WF-020: SET_KVP + SET_MAS + LOAD_APR commands.</remarks>
    Task<Result> PrepareAsync(ExposureParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the EXPOSE command after generator reports READY state.
    /// Blocks until exposure is complete or timeout occurs.
    /// </summary>
    /// <remarks>SWR-WF-021: PREP → EXPOSE command sequence. Timeout: 30 seconds.</remarks>
    Task<Result> TriggerExposureAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the ABORT command immediately. Must be callable from any thread.
    /// </summary>
    /// <remarks>SWR-WF-022: ABORT command. HAZ-RAD safety interlock — must not fail silently.</remarks>
    Task<Result> AbortAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries current generator status (heat units, ready state).
    /// </summary>
    Task<Result<GeneratorStatus>> GetStatusAsync(CancellationToken cancellationToken = default);
}
