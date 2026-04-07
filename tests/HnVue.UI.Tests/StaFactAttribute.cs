using System;
using System.Threading;
using Xunit;
using Xunit.Sdk;

namespace HnVue.UI.Tests;

/// <summary>
/// xUnit [Fact] variant that runs the test on an STA thread.
/// Required for WPF component instantiation which demands STA thread apartment state.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("HnVue.UI.Tests.StaFactDiscoverer", "HnVue.UI.Tests")]
public sealed class StaFactAttribute : FactAttribute { }

/// <summary>
/// xUnit [Theory] variant that runs each data row on an STA thread.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer("HnVue.UI.Tests.StaTheoryDiscoverer", "HnVue.UI.Tests")]
public sealed class StaTheoryAttribute : DataAttribute
{
    /// <inheritdoc />
    public override System.Collections.Generic.IEnumerable<object[]> GetData(System.Reflection.MethodInfo testMethod)
        => Array.Empty<object[]>();
}

/// <summary>
/// Helper to run a test action on a dedicated STA thread.
/// </summary>
internal static class StaRunner
{
    /// <summary>
    /// Executes <paramref name="action"/> on a new STA thread and re-throws
    /// any exception on the calling thread.
    /// </summary>
    internal static void Run(Action action)
    {
        Exception? caught = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                caught = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (caught is not null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(caught).Throw();
    }
}
