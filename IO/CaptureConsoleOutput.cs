// Copyright Joseph W Donahue and Sharper Hacks LLC (US-WA)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// SharperHacks is a trademark of Sharper Hacks LLC (US-Wa), and may not be
// applied to distributions of derivative works, without the express written
// permission of a registered officer of Sharper Hacks LLC (US-WA).

using System.Diagnostics.CodeAnalysis;

namespace SharperHacks.IO;

/// <summary>
/// Console output redirection wrapper with IDisposable implementation.
/// </summary>
public class CaptureConsoleOutput : IDisposable
{
    #region Public

    // TODO: Add clear captured output. Maybe add a DrainOutput property?

    /// <summary>
    /// The captured output.
    /// </summary>
    [NotNull]
    public string CapturedOutput => _stringWriter?.ToString() ?? string.Empty;

    #region Constructors

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// Thread and nested using() block safe.
    /// </remarks>
    public CaptureConsoleOutput() 
    {
        Initialize(-1); // Infinite wait.
    }

    /// <summary>
    /// Constructor with timeout on internally shared, mutex synchronized resource wait.
    /// </summary>
    /// <remarks>
    /// Thread and nested using() block safe.
    /// Throws TimeoutException if time to wait expires.
    /// </remarks>
    /// <param name="millisecondsToWait"></param>
    public CaptureConsoleOutput(int millisecondsToWait)
    {
        Initialize(millisecondsToWait);
    }

    #endregion Constructors

    #endregion Public


    #region Private

    [NotNull] private static readonly object _staticLock = new();
    private bool _initialized;

    [NotNull] private static readonly Mutex _mutex = new();

    // The thread that currently owns console output redirection.
    private static int _currentThreadId;

    private static ulong _nestedCounter;

    private StringWriter? _stringWriter;
    private TextWriter? _previousConsoleOut;

    private void Initialize(int millisecondsToWait)
    {

        var myThreadId = Environment.CurrentManagedThreadId;

        lock (_staticLock)
        {
            if (!_initialized)
            {
                _initialized = true;
            }

            if (_currentThreadId == myThreadId)
            {
                // We're already holding the mutex.
                _nestedCounter++;
                Redirect();
                return;
            }
        }

        if (!_mutex.WaitOne(millisecondsToWait))
        {
            throw new TimeoutException("Timed-out waiting on mutex.");
        }

        lock (_staticLock)
        {
            _currentThreadId = myThreadId;
        }
        Redirect();
    }

    private void Redirect()
    {
        // Redirect console output so we can capture it.
        _stringWriter = new StringWriter();
        lock (_staticLock)
        {
            _previousConsoleOut = Console.Out;
            Console.SetOut(_stringWriter);
        }
    }

    #endregion Private

    #region IDisposable Support

    private bool _disposedValue; // To detect redundant calls

    /// <summary>
    /// Ensures console output is restored to previous stream.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _disposedValue = true;

            if (disposing)
            {
                // Restore console output.
                lock (_staticLock)
                {
                    if (null != _previousConsoleOut)
                    {
                        Console.SetOut(_previousConsoleOut);
                    }

                    _currentThreadId = 0;

                    if (_nestedCounter == 0)
                    {
                        _mutex.ReleaseMutex();
                    }
                    else
                    {
                        _nestedCounter--;
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    ~CaptureConsoleOutput()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}