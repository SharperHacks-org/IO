// Copyright and trademark notices at the end of this file.

using SharperHacks.CoreLibs.Constraints;

using System.Diagnostics.CodeAnalysis;

namespace SharperHacks.CoreLibs.IO;

/// <summary>
/// Manages lifetime of temporary directories with Path.GetTempPath() at their root.
/// </summary>
/// <remarks>
/// Each instance has a unique name and will be deleted when the object is disposed.
/// Each instance has Path.GetTempPath() as it's root.
/// </remarks>
public class TempDirectory : IDisposable
{
    /// <summary>
    /// The DirectoryInfo object obtained on successful directory creation.
    /// </summary>
    [NotNull]
    public DirectoryInfo DirectoryInfo { get; }

    #region IDisposable

    private bool _disposedValue; // To detect redundant calls

    /// <summary>
    /// Implements the dispose pattern.
    /// </summary>
    /// <param name="disposing">
    /// Currently ignored. No managed objects to dispose.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _disposedValue = true;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (disposing && null != DirectoryInfo && Directory.Exists(DirectoryInfo.FullName))
            {
                Directory.Delete(DirectoryInfo.FullName, true);
            }
        }
    }

    /// <summary>
    /// Finalizer implements the Dispose pattern.
    /// </summary>
    ~TempDirectory()
    {
        Dispose(!_disposedValue);
    }

    /// <summary>
    /// Implements the Dispose pattern.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // Because we override the finalizer.
        GC.SuppressFinalize(this);
    }
    #endregion IDisposable

    #region Constructors

    /// <summary>
    /// Default constructor creates a new GUID string that is used to create
    /// a directory in the Path.GetTempPath() directory.
    /// </summary>
    public TempDirectory(params string[] subDirs) => DirectoryInfo = CreateDirectory("", subDirs);

    /// <summary>
    /// Constructor uses the prefix and a new GUID string to create a directory
    /// in the Path.GetTempPath() directory.
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="subDirs"></param>
    public TempDirectory(string prefix, params string[] subDirs)
    {
        Verify.IsNotNull(prefix);
        DirectoryInfo = CreateDirectory(prefix, subDirs);
    }

    /// <summary>
    /// Mounts the existing sub-directory specified by dirInfo, and
    /// removes that directory when disposed.
    /// </summary>
    /// <param name="dirInfo"></param>
    /// <param name="subDirs"></param>
    /// <returns></returns>
    public TempDirectory(DirectoryInfo dirInfo, params string[] subDirs)
    {
        Verify.IsNotNull(dirInfo);
        DirectoryInfo = dirInfo;

        if (subDirs.Length > 0)
        {
            CreateSubDirs(dirInfo.FullName, subDirs);
        }
    }
    #endregion Constructors

    #region Public methods

    /// <summary>
    /// Creates a sub-directory of the given name.
    /// </summary>
    /// <param name="name">Name of the new subdirectory.</param>
    /// <returns>New DirectoryInfo object.</returns>
    public DirectoryInfo CreateNamedSubdirectory(string name)
    {
        Verify.IsNotNull(name);

        var result = DirectoryInfo.CreateSubdirectory(name);

        return result;
    }

    /// <summary>
    /// Creates a sub-directory using a new GUID string.
    /// </summary>
    /// <returns>New DirectoryInfo object.</returns>
    public DirectoryInfo CreateSubdirectory()
    {
        var result = DirectoryInfo.CreateSubdirectory(Guid.NewGuid().ToString());

        return result;
    }

    /// <summary>
    /// Creates a sub-directory using the prefix and a new GUID string.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns>New DirectoryInfo object.</returns>
    public DirectoryInfo CreateSubdirectory(string prefix) => DirectoryInfo.CreateSubdirectory(prefix + Guid.NewGuid());

    /// <summary>
    /// Combines the current temp path with the provided prefix and a GUID string.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public static string CreateUniqueTempPathString(string prefix)
    {
        var result = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid());

        return result;
    }

    /// <summary>
    /// Delete all files in this temp directory.
    /// </summary>
    public void DeleteAllFiles()
    {
        IOException? firstEx = null;

        foreach (var file in Directory.GetFiles(DirectoryInfo.FullName, "*", SearchOption.AllDirectories)!)
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException ex)
            {
                firstEx ??= ex;
                // Don't rethrow here, so we can delete as many files as possible.
            }
        }

        if (null != firstEx)
        {
            throw firstEx;
        }
    }
    #endregion Public methods

    #region Private

    private static void CreateSubDirs(string root, IEnumerable<string> subDirs)
    {
        foreach (var subDir in subDirs)
        {
            var path = Path.Combine(root, subDir);
            Verify.IsTrue(path.StartsWith(root, StringComparison.InvariantCultureIgnoreCase));
            _ = Directory.CreateDirectory(path);
        }
    }
    private static DirectoryInfo CreateDirectory([NotNull] string prefix, params string[] subDirs)
    {
        Verify.IsFalse(prefix.Contains(".."));

        string path;

        // We loop until we create a path that does not yet exist.
        // In the life time of this code, somewhere in the universe, this loop
        // might iterate more than once, however unlikely, this loop is required.
        do
        {
            path = CreateUniqueTempPathString(prefix);
        } while (Directory.Exists(path));

        var result = Directory.CreateDirectory(path)!;

        if (subDirs.Length > 0)
        {
            CreateSubDirs(result.FullName, subDirs);
        }

        return result;
    }
    
    #endregion Private
}

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
