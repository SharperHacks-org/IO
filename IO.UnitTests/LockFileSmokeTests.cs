// Copyright and trademark notices at the end of this file.

using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharperHacks.CoreLibs.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class LockFileSmokeTests
{
    private static int FinalizerHelperWithDispose(FileInfo fileInfo)
    {
        using var lockFile = new LockFile(fileInfo.FullName);

        var result = GC.GetGeneration(lockFile);

        return result;
    }

    private static int FinalizerHelperWithoutDispose(FileInfo fileInfo)
    {
        var lockFile = new LockFile(fileInfo.FullName);

        var result = GC.GetGeneration(lockFile);

        return result;
    }

    [TestMethod]
    public void Finalizer()
    {
        using var tempDir = new TempDirectory(nameof(LockFileSmokeTests));
        using var tempFile = new TempFile(tempDir.DirectoryInfo);

        tempFile.FileStream.Close();

        var generation = FinalizerHelperWithoutDispose(tempFile.FileInfo);
        GC.Collect(generation, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();

        generation = FinalizerHelperWithDispose(tempFile.FileInfo);
        GC.Collect(generation, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();
    }

    private static string _lockFileName = string.Empty;
    private static bool _exceptionCaught;

    public static void LockFileThreadFunc()
    {
        try
        {
            using var lockFile = new LockFile(_lockFileName, 10, 2, 10, 1);
        }
        catch (TimeoutException)
        {
            _exceptionCaught = true;
        }
    }

    [TestMethod]
    public void ConstructorIOExceptionBranch()
    {
        {
            using var tempDir = new TempDirectory(nameof(LockFileSmokeTests));
            using var tempFile = new TempFile(
                    tempDir.DirectoryInfo,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.Read);

            _lockFileName = tempFile.FileInfo.FullName;

            var thread = new Thread(LockFileThreadFunc);
            thread.Start();
            thread.Join();
            Assert.IsTrue(_exceptionCaught);
        }

        Assert.IsFalse(File.Exists(_lockFileName));
    }
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
