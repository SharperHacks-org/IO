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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharperHacks.CoreLibs.Constraints;

namespace SharperHacks.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class FileHelpersSmokeTests //: TestBase
{
    [TestMethod]
    public void SplitPattern()
    {
        (string prefix, string specifier, string postfix)[] patternElements = new[]
        {
            ("prefix1", "{n}", "postfix1"), ("prefix2\blah", "{n}", "postfix2.blah"),
            (@"d:\prefix3\blah\blah", "{n}", "postfix2.blah.blah"),
            (@"d:\prefix3\blah\blah", "{n}", "postfix2.blah.blah"),
            (string.Empty, "{yak yak}", string.Empty)
        };

        foreach (var (prefix, specifier, postfix) in patternElements)
        {
            var (prefixResult, specifierResult, postfixResult) =
                    FileHelpers.SplitPattern(prefix + specifier + postfix);
            Assert.AreEqual(prefix, prefixResult);
            Assert.AreEqual(specifier, '{' + specifierResult + '}');
            Assert.AreEqual(postfix, postfixResult);
        }

        var exceptionCaught = false;
        try
        {
            _ = FileHelpers.SplitPattern("prefix}postfix"); // missing left brace.
            Assert.Fail("Previous line should have thrown ArgumentException.");
        }
        catch (ArgumentException ex)
        {
            exceptionCaught = true;
            Assert.IsTrue(ex.Message.Contains("Missing specifier."));
        }
        Assert.IsTrue(exceptionCaught);

        exceptionCaught = false;
        try
        {
            _ = FileHelpers.SplitPattern("prefix{postfix"); // missing right brace.
            Assert.Fail("Previous line should have thrown ArgumentException.");
        }
        catch (ArgumentException ex)
        {
            exceptionCaught = true;
            Assert.IsTrue(ex.Message.Contains("Malformed specifier"));
        }
        Assert.IsTrue(exceptionCaught);

        exceptionCaught = false;
        try
        {
            _ = FileHelpers.SplitPattern(null!); // Will throw VerifyException
            Assert.Fail("Previous line should have thrown ArgumentException.");
        }
        catch (VerifyException)
        {
            exceptionCaught = true;
        }
        Assert.IsTrue(exceptionCaught);
    }

    // Fill any gaps in the numbered file name sequence first..last.
    private static void CreateNumberedFilesInRange(long first, long last, string rootPath, string prefix, string postfix)
    {
        Verify.IsTrue(last - first < 1000);

        for (var idx = first; idx <= last; idx++)
        {
            // Break the loop when idx overflows.
            if (long.MinValue == idx) break;

            var numberedFileName = prefix + idx.ToString() + postfix;
            var pathFileName = Path.Join(rootPath, numberedFileName);
            if (!File.Exists(pathFileName)) File.Create(pathFileName).Close();
        }
    }

    private static bool _oneTimeTestsPerformed;

    private static void TestRange(long first, long last, DirectoryInfo dirInfo, string prefix, string postfix)
    {
        Verify.IsTrue(first <= last);

        var rootPath = dirInfo.FullName; //dirInfo == null ? string.Empty : dirInfo.FullName;

        CreateNumberedFilesInRange(first, last, rootPath, prefix, postfix);
        var filesFound = dirInfo.GetFiles("*").Length;
        Verify.IsTrue(filesFound >= last - first + 1);

        var pathSeparator = Path.DirectorySeparatorChar;

        // Test both overloads happy paths.
        Assert.AreEqual(last, FileHelpers.HighestN(Path.Join(rootPath, pathSeparator + prefix), postfix));
        Assert.AreEqual(last, FileHelpers.HighestN(Path.Join(rootPath, pathSeparator + prefix) + "{n}" + postfix));

        // The pattern overload also has an unhappy path to test...
        var exceptionCaught = false;

        if (!_oneTimeTestsPerformed)
        {
            try
            {
                _ = FileHelpers.HighestN(Path.Join(rootPath, pathSeparator + prefix) + "{INVALID_SPECIFIER}" + postfix);
                Assert.Fail("Failed to throw expected ArgumentException");
            }
            catch (ArgumentException ex)
            {
                exceptionCaught = true;
                Verify.IsTrue(ex.Message.Contains(FileHelpers.InvalidSpecifierExceptionMsg));
            }
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Verify.IsTrue(exceptionCaught);

            _oneTimeTestsPerformed = true;
        }
    }

    private static void CleanAndVeryTempDir(TempDirectory tmpDir)
    {
        tmpDir.DeleteAllFiles();
        Verify.AreEqual(0, tmpDir.DirectoryInfo.GetFiles("*").Length);
    }

    [TestMethod]
    public void TestHighestN()
    {
        const string filePrefix = "pre-";
        const string postfix = "-post.tmp";

        using var tmpDir1 = new TempDirectory("HighestNSmokeTest");
        Verify.AreEqual(0, tmpDir1.DirectoryInfo.GetFiles("*").Length);

        // Just one and highest is 0.
        TestRange(0, 0, tmpDir1.DirectoryInfo, filePrefix, postfix);
        
        // Count will be 4 with a gap of 1..6 inclusive, making highest 9.
        TestRange(7, 9, tmpDir1.DirectoryInfo, filePrefix, postfix);
        
        // Count will be 10, no gaps, highest is 9.
        TestRange(0, 9, tmpDir1.DirectoryInfo, filePrefix, postfix);

        CleanAndVeryTempDir(tmpDir1);

        // Count will be 20, with a gap at 10, and highest 21.
        TestRange(11, 21, tmpDir1.DirectoryInfo, filePrefix, postfix);
        CleanAndVeryTempDir(tmpDir1);

        // Count will be 420, with a gap at 22..100, and highest 500.
        TestRange(100, 500, tmpDir1.DirectoryInfo, filePrefix, postfix);

        tmpDir1.DeleteAllFiles();

        // Verify that we can have just one, that isn't zero.
        TestRange(long.MaxValue, long.MaxValue, tmpDir1.DirectoryInfo, filePrefix, postfix);

        // A file can match the search glob "prefix*postfix" and not be a numbered file.
        // Push into the code paths handling files that match prefix
        // and postfix with non-numeric noise or nothing in the middle.
        using var tmpDir2 = new TempDirectory("HighestNSmokeTest-Dirty-");
        const string altPostfix = ".txt";
        File.Create(Path.Join(tmpDir2.DirectoryInfo.FullName, "3a4" + altPostfix)).Close();
        File.Create(Path.Join(tmpDir2.DirectoryInfo.FullName, "42" + altPostfix + "noise")).Close(); // Dirty file extension.
        File.Create(Path.Join(tmpDir2.DirectoryInfo.FullName, altPostfix)).Close();
        File.Create(Path.Join(tmpDir2.DirectoryInfo.FullName, altPostfix + "noise")).Close();
        TestRange(0, 5, tmpDir2.DirectoryInfo, string.Empty, altPostfix);
    }

    private static void TestSplitPathFromFileNamePrefix(
            string path, 
            string fileNamePrefix, 
            string expectedPath, 
            string expectedFileNamePrefix)
    {
        var (resultPath, resultFileNamePrefix) = FileHelpers.SplitPathFromFileNamePrefix(Path.Join(path, fileNamePrefix));
        Assert.AreEqual(expectedPath, resultPath);
        Assert.AreEqual(expectedFileNamePrefix, resultFileNamePrefix);
    }

    [TestMethod]
    public void SplitPathFromFileNamePrefix()
    {
        var path = string.Empty;
        var fileNamePrefix = string.Empty;

        // Verify that we can process pure numeric file names in the CWD.
        TestSplitPathFromFileNamePrefix(path, fileNamePrefix, ".", fileNamePrefix);

        // Verify that we can process pure numeric file names in the specified directory.
        path = Path.GetTempPath();
        TestSplitPathFromFileNamePrefix(path, fileNamePrefix, path, fileNamePrefix);

        // Verify that we can process numeric file names containing a prefix in the specified directory.
        fileNamePrefix = "pre-";
        TestSplitPathFromFileNamePrefix(path, fileNamePrefix, path, fileNamePrefix);
    }
}