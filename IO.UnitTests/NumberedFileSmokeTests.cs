// Copyright and trademark notices at the end of this file.

using System.Diagnostics.CodeAnalysis;

using SharperHacks.CoreLibs.Reflection;

namespace SharperHacks.CoreLibs.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class NumberedFileSmokeTests
{
    [TestMethod]
    public void SmokeIntegerAutoIncrementCreate_DefaultArgs()
    {
        try
        {
            using var tmpDir = new TempDirectory(Code.MemberName());
            var originalDir = Directory.GetCurrentDirectory();

            Directory.SetCurrentDirectory(tmpDir.DirectoryInfo.FullName);

            Console.WriteLine($"Temp dir: {tmpDir.DirectoryInfo.FullName}");
            Console.WriteLine($"Original test dir: {originalDir}");
            Console.WriteLine($"New CWD: {Directory.GetCurrentDirectory()}");

            // Test numbered file names
            for (var count = 1; count < 4; count++)
            {
                var result = NumberedFile.IntegerAutoIncrementCreate();
                Assert.IsTrue(File.Exists(count.ToString()));
                Assert.IsTrue(File.Exists(result.Name));
                Assert.IsTrue(result.CanRead);
                Assert.IsTrue(result.CanWrite);
                Assert.IsTrue(result.CanSeek);
                result.Close();
            }

            Directory.SetCurrentDirectory(originalDir);
            tmpDir.DeleteAllFiles();
        }
        catch(Exception ex)
        {
            Assert.Fail($"Unexpected exception: {ex.Message}");
        }
    }

    [TestMethod]
    public void SmokeIntegerAutoIncrementCreate_SpecifiedArgs()
    {
        try
        {
            using var tmpDir = new TempDirectory(Code.MemberName());

            const string fileName = "fn";
            const string extension = "ext";
            const string separator = ".";
            const int floor = 99;
            const int fileCount = 3;

            // Test numbered file names
            for (var count = floor; count < floor + fileCount; count++)
            {
                var result = NumberedFile.IntegerAutoIncrementCreate(
                        tmpDir.DirectoryInfo.FullName,
                        fileName,
                        extension,
                        separator,
                        floor
                        );
                Assert.Contains(count.ToString(), result.Name);
                Assert.IsTrue(File.Exists(result.Name));
                Assert.IsTrue(result.CanRead);
                Assert.IsTrue(result.CanWrite);
                Assert.IsTrue(result.CanSeek);
                result.Close();
            }

            // Test that we get a new higher number when an existing file matches floor.
            using var res2 = NumberedFile.IntegerAutoIncrementCreate(
                    tmpDir.DirectoryInfo.FullName,
                    fileName,
                    extension,
                    separator,
                    floor
                    );
            var fileToLookFor = Path.Combine(tmpDir.DirectoryInfo.FullName, fileName)
                              + separator
                              + (floor + fileCount)
                              + "."
                              + extension;

            Assert.IsTrue(File.Exists(fileToLookFor));
            res2.Close();

            tmpDir.DeleteAllFiles();
        }
        catch(Exception ex)
        {
            Assert.Fail(ex.Message);
        }
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
