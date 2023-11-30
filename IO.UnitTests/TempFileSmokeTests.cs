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
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharperHacks.CoreLibs.Constraints;

namespace SharperHacks.CoreLibs.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class TempFileSmokeTests //: TestBase
{
    private static string CheckExists(TempFile tempFile)
    {
        Verify.IsNotNull(tempFile);
        var filePathName = tempFile.FileInfo.FullName;
        Assert.IsTrue(File.Exists(filePathName));

        return filePathName;
    }
    private static string CheckExistsAndInTempPath(TempFile tempFile)
    {
        var filePathName = CheckExists(tempFile);
        Assert.IsTrue(filePathName.StartsWith(Path.GetTempPath()));

        return filePathName;
    }

    private static string CheckExistsAndInTempDirectory(TempFile tempFile, TempDirectory tempDirectory)
    {
        Verify.IsNotNull(tempDirectory);
        var filePathName = CheckExists(tempFile);
        Assert.IsTrue(File.Exists(filePathName));
        Assert.IsTrue(filePathName.StartsWith(tempDirectory.DirectoryInfo.FullName));

        return filePathName;
    }

    [TestMethod]
    public void ConstructorDefault()
    {
        string filePathName;

        using (var tempFile = new TempFile())
        {
            filePathName = CheckExistsAndInTempPath(tempFile);
        }
        Assert.IsFalse(File.Exists(filePathName));
    }

    [TestMethod]
    public void ConstructorFQPN()
    {
        string fqpn;
        using (var tempDir = new TempDirectory())
        {
            using var tempFile = new TempFile(Path.Combine(tempDir.DirectoryInfo.FullName, new Guid().ToString("N")));
            Assert.IsTrue(File.Exists(tempFile.FileInfo.FullName));
            fqpn = tempFile.FileInfo.FullName;
        }
        Assert.IsFalse(File.Exists(fqpn));
    }

    [TestMethod]
    public void ConstructorPrefixExtension()
    {
        string filePathName;

        using (var tempFile = new TempFile(string.Empty, string.Empty))
        {
            filePathName = CheckExistsAndInTempPath(tempFile);
            Assert.IsTrue(string.IsNullOrEmpty(tempFile.FileInfo.Extension));
        }
        Assert.IsFalse(File.Exists(filePathName));

        var prefix = "Prefix1";
        using (var tempFile = new TempFile(prefix, string.Empty))
        {
            filePathName = CheckExistsAndInTempPath(tempFile);
            Assert.IsTrue(tempFile.FileInfo.Name.StartsWith(prefix));
            Assert.IsTrue(string.IsNullOrEmpty(tempFile.FileInfo.Extension));
        }
        Assert.IsFalse(File.Exists(filePathName));

        var extension = "ext1";
        using (var tempFile = new TempFile(prefix, extension))
        {
            filePathName = CheckExistsAndInTempPath(tempFile);
            Assert.IsTrue(tempFile.FileInfo.Name.StartsWith(prefix));
            Assert.IsTrue(tempFile.FileInfo.Extension.StartsWith('.'));
            Assert.IsTrue(tempFile.FileInfo.Extension.EndsWith(extension));
            var tokens = tempFile.FileInfo.FullName.Split('.');
            Assert.AreEqual(2, tokens.Length);
        }
        Assert.IsFalse(File.Exists(filePathName));

        extension = "ext2";
        using (var tempFile = new TempFile(string.Empty, extension))
        {
            filePathName = CheckExistsAndInTempPath(tempFile);
            Assert.IsTrue(tempFile.FileInfo.Extension.StartsWith('.'));
            Assert.IsTrue(tempFile.FileInfo.Extension.EndsWith(extension));
            var tokens = tempFile.FileInfo.FullName.Split('.');
            Assert.AreEqual(2, tokens.Length);
        }
        Assert.IsFalse(File.Exists(filePathName));
    }

#if false
    [TestMethod]
    public void Constructor_PrefixExtension_HasUserDataChecks()
    {
        bool exceptionCaught = false;
        try
        {
            using (var tempFile = new TempFile(@"..\invalidPrefix", null))
            {}
        }
        catch (VerifyException)
        {
            exceptionCaught = true;
        }
        Verify.IsTrue(exceptionCaught);
        exceptionCaught = false;

        try
        {
            using (var tempFile = new TempFile(null, @"\..\badExtension"))
            { }
        }
        catch (VerifyException)
        {
            exceptionCaught = true;
        }
        Verify.IsTrue(exceptionCaught);
    }
#endif

    [TestMethod]
    public void Constructor_DirectoryInfo()
    {
        string filePathName;

        using (var tempDirectory = new TempDirectory())
        {
            using var tempFile = new TempFile(tempDirectory.DirectoryInfo);
            filePathName = CheckExistsAndInTempDirectory(tempFile, tempDirectory);
        }
        Assert.IsFalse(File.Exists(filePathName));
    }

    [TestMethod]
    public void Constructor_DirectoryInfoPrefixExtension()
    {
        var prefix = "prefix";
        var extension = "ext";

        string filePathName;

        using (var tempDirectory = new TempDirectory())
        {
            using var tempFile = new TempFile(tempDirectory.DirectoryInfo, prefix, extension);
            filePathName = CheckExistsAndInTempDirectory(tempFile, tempDirectory);
            Assert.IsTrue(tempFile.FileInfo.Name.StartsWith(prefix));
            Assert.IsTrue(tempFile.FileInfo.Extension.EndsWith(extension));
        }
        Assert.IsFalse(File.Exists(filePathName));
    }

    [TestMethod]
    public void VerifyTempFileIsUseful()
    {
        using var tempFile = new TempFile();

        Assert.IsTrue(tempFile.FileStream.CanWrite);
        Assert.IsTrue(tempFile.FileStream.CanRead);
        Assert.IsTrue(tempFile.FileStream.CanSeek);

        var testString = "This is a test.";

        using (var sw = new StreamWriter(tempFile.FileStream, Encoding.ASCII))
        {
            sw.WriteLine(testString);
        }

        using var sr = new StreamReader(tempFile.FileInfo.FullName, Encoding.ASCII);
        var line = sr.ReadLine();
        Assert.AreEqual(testString, line);
    }

    private static int FinalizerHelperWithDispose()
    {
        using var tempFile = new TempFile();

        var result = GC.GetGeneration(tempFile);

        return result;
    }

    static TempFile? _undisposed;

    private static int FinalizerHelperWithoutDispose()
    {
        _undisposed = new TempFile();
        var result = GC.GetGeneration(_undisposed);

        return result;
    }

    [TestMethod]
    public void Finalizer()
    {
#if false
        var generation = FinalizerHelperWithoutDispose();
        GC.Collect(generation, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();
#endif
        var generation = FinalizerHelperWithDispose();
        GC.Collect(generation, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();
    }
}
