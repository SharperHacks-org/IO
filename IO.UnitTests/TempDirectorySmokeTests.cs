// Copyright Joseph W Donahue and SharperHacks LLC (US-WA)
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
// SharperHacks is a trademark of SharperHacks LLC (US-Wa), and may not be
// applied to distributions of derivative works, without the express written
// permission of a registered officer of SharperHacks LLC (US-WA).

using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharperHacks.Constraints;

namespace SharperHacks.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class TempDirectorySmokeTests //: TestBase
{
    private readonly string[] _subDirs = new[]
    {
        @"A1",
        @"A1\A2",
        @"A1\A2\A3",
        @"B",
        @"C1",
        @"C1\C2"
    };

    [TestMethod]
    public void Constructor_Default()
    {
        string directoryPath;

        using (var tmpDir = new TempDirectory())
        {
            directoryPath = tmpDir.DirectoryInfo.FullName;
            Assert.IsTrue(Directory.Exists(directoryPath));
        }

        Assert.IsFalse(Directory.Exists(directoryPath));

        using (var tmpDir = new TempDirectory(_subDirs))
        {
            var dirRoot = tmpDir.DirectoryInfo.FullName;
            var tmpRoot = Path.GetTempPath();
            Assert.IsTrue(dirRoot.StartsWith(tmpRoot));

            foreach (var subDir in _subDirs)
            {
                var expectedPath = Path.Combine(dirRoot, subDir);
                Assert.IsTrue(Directory.Exists(expectedPath));
            }
        }
    }

    [TestMethod]
    public void Constructor_Prefix()
    {
        var prefix = "SmokeTest-ConstructorPrefix";
        string directoryPath;

        using (var tmpDir = new TempDirectory(prefix))
        {
            directoryPath = tmpDir.DirectoryInfo.FullName;
            Assert.IsTrue(Directory.Exists(directoryPath));
            Assert.IsTrue(directoryPath.Contains(prefix));
        }

        Assert.IsFalse(Directory.Exists(directoryPath));

        using (var tmpDir = new TempDirectory(prefix, _subDirs))
        {
            var dirRoot = tmpDir.DirectoryInfo.FullName;
            var tmpRoot = Path.GetTempPath();
            Assert.IsTrue(dirRoot.StartsWith(tmpRoot));

            foreach (var subDir in _subDirs)
            {
                var expectedPath = Path.Combine(dirRoot, subDir);
                Assert.IsTrue(Directory.Exists(expectedPath));
            }
        }
    }

    [TestMethod]
    public void Constructor_DirectoryInfo()
    {
        var prefix = "SmokeTest-ConstructorDirInfo";
        var directoryPath = TempDirectory.CreateUniqueTempPathString(prefix);
        var dirInfo = Directory.CreateDirectory(directoryPath);

        Assert.IsTrue(directoryPath != null && directoryPath.Contains(prefix));

        using (var tmpDir = new TempDirectory(dirInfo!))
        {
            Assert.IsTrue(Directory.Exists(tmpDir.DirectoryInfo.FullName));
        }

        Assert.IsFalse(Directory.Exists(directoryPath!));

        using (var tmpDir = new TempDirectory(dirInfo, _subDirs))
        {
            var dirRoot = tmpDir.DirectoryInfo.FullName;
            var tmpRoot = Path.GetTempPath();
            Assert.IsTrue(dirRoot.StartsWith(tmpRoot));

            foreach (var subDir in _subDirs)
            {
                var expectedPath = Path.Combine(dirRoot, subDir);
                Assert.IsTrue(Directory.Exists(expectedPath));
            }
        }
    }

    [TestMethod]
    [ExpectedException(typeof(VerifyException))]
    public void Constructor_RelativePathPrefixRejected()
    {
        var prefix = @"..\deleteMe";
        // ReSharper disable UnusedVariable
        using var tmpDir = new TempDirectory(prefix);
    }

    [TestMethod]
    public void CreateSubdirectoriesAndFiles()
    {
        var prefix = "SmokeTest-" + nameof(CreateSubdirectoriesAndFiles);
        var subDirectoryName = "SmokeTest-NamedSubDirectory";
        string directoryPath;
        var subdirectories = new List<string>();

        using (var tmpDir = new TempDirectory(prefix))
        {
            directoryPath = tmpDir.DirectoryInfo.FullName;
            Assert.IsTrue(Directory.Exists(directoryPath));
            Assert.IsTrue(directoryPath.Contains(prefix));

            subdirectories.Add(tmpDir.CreateSubdirectory().FullName);
            subdirectories.Add(tmpDir.CreateSubdirectory(prefix).FullName);
            subdirectories.Add(tmpDir.CreateNamedSubdirectory(subDirectoryName).FullName);

            Assert.AreEqual(3, subdirectories.Count);
            
            foreach (var path in subdirectories)
            {
                Assert.IsTrue(Directory.Exists(path));
                File.Create(Path.Combine(path!, "TestFile.delete.me")).Close();
            }
        }

        foreach (var path in subdirectories)
        {
            Assert.IsFalse(Directory.Exists(path));
        }

        Assert.IsFalse(Directory.Exists(directoryPath));
    }

    [TestMethod]
    public void DeleteAllFilesThrowsIOException()
    {
        var pathToDelete = string.Empty;
        FileStream? fs = null;
        var exceptionCaught = false;

        try
        {
            using (var tmpDir = new TempDirectory(nameof(DeleteAllFilesThrowsIOException)))
            {
                pathToDelete = tmpDir.DirectoryInfo.FullName;
                fs = File.Create(Path.Combine(tmpDir.DirectoryInfo.FullName, "deleteMe.txt"));
                tmpDir.DeleteAllFiles();
            }

            Assert.Fail("An IOException should prevent execution of this statement.");
        }
        catch (IOException)
        {
            exceptionCaught = true;
        }
        finally
        {
            fs?.Close();
            Directory.Delete(pathToDelete, true);
        }
        Assert.IsTrue(exceptionCaught);
    }

    private static int FinalizerHelperWithDispose()
    {
        using var tempDirectory = new TempDirectory();

        var result = GC.GetGeneration(tempDirectory);

        return result;
    }

    private static int FinalizerHelperWithoutDispose()
    {
        var tempDirectory = new TempDirectory();
        Assert.IsNotNull(tempDirectory);
        var result = GC.GetGeneration(tempDirectory);

        return result;
    }

    [TestMethod]
    public void Finalizer()
    { 
        var generation = FinalizerHelperWithoutDispose();
        GC.Collect(generation, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();

        generation = FinalizerHelperWithDispose();
        GC.Collect(generation, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();
    }
}
