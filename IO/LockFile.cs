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

using SharperHacks.CoreLibs.Constraints;

namespace SharperHacks.CoreLibs.IO;

/// <summary>
/// Uses a file as a semaphore between threads and processes.
/// </summary>
public class LockFile : IDisposable
{
    #region Constructors

    /// <summary>
    /// Attempts to open or create <paramref name="pfn"/> with read and write access, no sharing.
    /// Retries for at least <paramref name="maxMilliseconds"/>.
    /// </summary>
    /// <param name="pfn"></param>
    /// <param name="maxMilliseconds"></param>
    /// <param name="delayIncrementCount"></param>
    /// <param name="firstLoopDelay"></param>
    /// <param name="delayMultiplier"></param>
    public LockFile( 
            string pfn, 
            int maxMilliseconds = int.MaxValue,
            double delayIncrementCount = 3,
            double firstLoopDelay = 50,
            double delayMultiplier = 2)
    {
        Verify.IsNotNullOrEmpty(pfn);

        _theFileName = Path.GetFullPath(pfn); // So we're not resolving relative paths later on.

        var deadLine = DateTime.Now.AddMilliseconds(maxMilliseconds);
        var currentDelay = firstLoopDelay;
        var counter = 0;

        while (DateTime.Compare(DateTime.Now, deadLine) < 1)
        {
            try
            {
                _theFile = new FileStream(
                        _theFileName, 
                        FileMode.OpenOrCreate, 
                        FileAccess.ReadWrite,
                        FileShare.None);

                return;
            }
            catch (IOException)
            {
                Thread.Sleep((int)currentDelay);

                if (counter++ < delayIncrementCount)
                {
                    currentDelay *= delayMultiplier;
                }
            }
        }

        throw new TimeoutException();
    }

    #endregion Constructors

    #region IDisposable

    /// <inheritdoc cref="IDisposable.Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_wasDisposed)
        {
            if (disposing)
            {
                _theFile.Close();
                File.Delete(_theFileName);
            }

            _wasDisposed = true;
        }
    }

    // <inheritdoc cref="IDisposable"/>
    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    //~LockFile() => Dispose(disposing: false);

    /// <inheritdoc cref="IDisposable.Dispose()"/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region Private
    private bool _wasDisposed;

    private readonly string _theFileName;
    private readonly FileStream _theFile;

    #endregion Private
}