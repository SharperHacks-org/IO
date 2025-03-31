// Copyright and trademark notices at the end of this file.

using SharperHacks.CoreLibs.Constraints;

using System.Diagnostics.CodeAnalysis;

namespace SharperHacks.CoreLibs.IO;

/// <summary>
/// Helper class for managing temporary files.
/// </summary>
/// <remarks>
/// After creating the file, the FileStream is left open to
/// hold the file.  
/// </remarks>
public class TempFile : IDisposable
{
    /// <summary>
    /// The FileInfo object obtained on successful file creation.
    /// </summary>
    [NotNull]
    public FileInfo FileInfo { get; private set; }

    /// <summary>
    /// The FileStream resulting from a successful file creation.
    /// </summary>
    [NotNull]
    public FileStream FileStream { get; private set; }

    /// <summary>
    /// Set true to diagnose leaked temporary files.
    /// </summary>
    public bool PropogateExceptionsOutOfDispose { get; set; }

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    /// <summary>
    /// Implements the dispose pattern.
    /// </summary>
    /// <param name="disposing">
    /// Disposes FileStream and deletes the file.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        try
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    FileStream.Close();
                    FileStream.Dispose();

                    FileStream = null!;
                }

                File.Delete(FileInfo.FullName);
                FileInfo = null!;

                _disposedValue = true;
            }
        }
        catch(IOException) 
        { 
            if (PropogateExceptionsOutOfDispose)
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Finalizer implements dispose pattern.
    /// </summary>
    ~TempFile()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.

        // Potential performance hit ensues if finalizer called before Dispose(),
        // but we do it anyway.
        FileStream.Close();
        Dispose(false);
    }

    /// <summary>
    /// Implements the dispose pattern.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // Because we override the finalizer.
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor creates new GUID string that is used to create
    /// an extensionless file in the Path.GetTempPath() directory. 
    /// </summary>
    public TempFile( 
            FileMode mode = FileMode.Create, 
            FileAccess access = FileAccess.ReadWrite, 
            FileShare share = FileShare.None)
    {
        var (fileInfo, fileStream) = CreateFile("", "", null, mode, access, share);
        Verify.IsNotNull(fileInfo);
        Verify.IsNotNull(fileStream);
        FileInfo = fileInfo;
        FileStream = fileStream;
    }

    /// <summary>
    /// Constructor uses fqpn (fully qualified path name) to create a file.
    /// </summary>
    /// <param name="fqpn"></param>
    /// <param name="mode"></param>
    /// <param name="access"></param>
    /// <param name="share"></param>
    public TempFile(
            string fqpn,
            FileMode mode = FileMode.Create, 
            FileAccess access = FileAccess.ReadWrite, 
            FileShare share = FileShare.None)
    {
        Verify.IsNotNull(fqpn);
        FileInfo = new FileInfo(fqpn);
        FileStream = new FileStream(fqpn, mode, access, share);
    }

    /// <summary>
    /// Constructor uses the prefix, new GUID string and extension to create
    /// a file in the Path.GetTempPath() directoryInfo.
    /// </summary>
    /// <remarks>
    /// Verifies that prefix and extension do not contain any double-dots ("..") so that
    /// user data can be used safely with this constructor. Other user and application temp
    /// directories cannot be reached into, indirectly.
    /// </remarks>
    /// <param name="prefix">
    /// The string to prefix to the file.
    /// May be empty or null.
    /// </param>
    /// <param name="extension">
    /// The extension to append to the file name.
    /// If leading dot is missing, one will be applied.
    /// May be empty or null.
    /// </param>
    /// <param name="mode"></param>
    /// <param name="access"></param>
    /// <param name="share"></param>
    public TempFile(
            string prefix,
            string extension,
            FileMode mode = FileMode.Create, 
            FileAccess access = FileAccess.ReadWrite, 
            FileShare share = FileShare.None)
    {
        Verify.IsNotNull(prefix);
        Verify.IsNotNull(extension);

        Verify.IsFalse(prefix.Contains(@".."));
        Verify.IsFalse(extension.Contains(@".."));

        var (fileInfo, fileStream) = CreateFile(prefix!, extension!, null, mode, access, share);
        Verify.IsNotNull(fileInfo);
        Verify.IsNotNull(fileStream);
        FileInfo = fileInfo;
        FileStream = fileStream;
    }

    /// <summary>
    /// Constructor, creates new GUID string that is used to create
    /// an extensionless file in the path specified by directoryInfo.
    /// </summary>
    /// <param name="directoryInfo"></param>
    /// <param name="mode"></param>
    /// <param name="access"></param>
    /// <param name="share"></param>
    public TempFile(
            DirectoryInfo directoryInfo,
            FileMode mode = FileMode.Create,
            FileAccess access = FileAccess.ReadWrite, 
            FileShare share = FileShare.None)
    {
        Verify.IsNotNull(directoryInfo);

        var (fileInfo, fileStream) = CreateFile("", "", directoryInfo.FullName, mode, access, share);
        Verify.IsNotNull(fileInfo);
        Verify.IsNotNull(fileStream);
        FileInfo = fileInfo;
        FileStream = fileStream;
    }

    /// <summary>
    /// Constructor, uses <paramref name="directoryInfo>"/> to place the temp file,
    /// <paramref name="prefix"/> and <paramref name="extension"/> to name it.
    /// </summary>
    /// <param name="directoryInfo"></param>
    /// <param name="prefix"></param>
    /// <param name="extension"></param>
    /// <param name="mode"></param>
    /// <param name="access"></param>
    /// <param name="share"></param>
    public TempFile(
            DirectoryInfo directoryInfo, 
            string prefix, 
            string extension,
            FileMode mode = FileMode.Create, 
            FileAccess access = FileAccess.ReadWrite, 
            FileShare share = FileShare.None)
    {
        Verify.IsNotNull(directoryInfo);

        var (fileInfo, fileStream) = CreateFile(prefix, extension, directoryInfo.FullName, mode, access, share);
        Verify.IsNotNull(fileInfo);
        Verify.IsNotNull(fileStream);
        FileInfo = fileInfo;
        FileStream = fileStream;
    }
    #endregion Constructors

    #region Private

    private static (FileInfo, FileStream) CreateFile(
            string prefix, 
            string extension, 
            string? path, 
            FileMode mode = FileMode.Create, 
            FileAccess access = FileAccess.ReadWrite, 
            FileShare share = FileShare.None)
    {
        string pathFileName;

        if (string.IsNullOrEmpty(path))
        {
            path = Path.GetTempPath();
        }

        do
        {
            var fileName = prefix + Guid.NewGuid().ToString();
            if (extension.Length > 0)
            {
                if (extension.First() != '.')
                {
                    fileName += '.';//_ = fileName.Append('.');
                }
                fileName += extension; // _ = fileName.Append(extension);
            }
            pathFileName = Path.Combine(path, fileName.ToString());
        } while (File.Exists(pathFileName));

        var fileInfo = new FileInfo(pathFileName);
        var fileStream = new FileStream(pathFileName, mode, access, share);

        return (fileInfo, fileStream);
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
