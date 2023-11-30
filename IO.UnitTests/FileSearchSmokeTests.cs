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

namespace SharperHacks.CoreLibs.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class FileSearchSmokeTests //: TestBase
{
    // TODO: Boiler plate here and DirectoriesSmokeTest is good candidate for a new class.
    private readonly string[] _subDirs = new[]
    {
        @"A1",
        @"A1\A2",
        @"A1\A2\A3",
        @"B",
        @"C1",
        @"C1\C2",
    };

    private TempDirectory GetPopulatedTempDir(string prefix, out List<string> dirList)
    {
        var tmpDir = new TempDirectory($"{nameof(GetPopulatedTempDir)}.{prefix}");
        dirList = new List<string>();
    
        foreach (var subDir in _subDirs)
        {
            var fqpn = Path.Join(tmpDir.DirectoryInfo.FullName, subDir);
            _ = Directory.CreateDirectory(fqpn);
            dirList.Add(fqpn);
        }
        Assert.IsTrue(dirList.Count != 0);

        return tmpDir;
    }


    private static HashSet<string> AddFiles(DirectoryInfo dirInfo)
    {
        var filesOracle = new HashSet<string>();

        foreach (var dir in Directory.GetDirectories(dirInfo.FullName))
        {
            if (dir.EndsWith("A1") || dir.EndsWith("A3") || dir.EndsWith("C2"))
            {
                for (var i = 0; i < 4; i++)
                {
                    var fqpn = Path.Join(dir, Guid.NewGuid().ToString("D"));
                    File.Create(fqpn).Close();
                    _ = filesOracle.Add(fqpn);
                }
            }
        }
        Assert.IsTrue(filesOracle.Count != 0);

        return filesOracle;
    }

    [TestMethod]
    public void DefaultConstructor()
    {
        var fs = new FileSearch(); // Default search pattern and directory.

        using var tmpDir = GetPopulatedTempDir(nameof(DefaultConstructor), out var dirs);
        Assert.IsNotNull(dirs);
        Assert.IsTrue(dirs.Count > 0);

        var fileList = new List<string>();
        foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
        {
            fileList.Add(file);
        }

        var count = 0;
        foreach(var fqpn in fs.GetFiles())
        {
            Assert.IsTrue(fileList.Contains(fqpn));
            count++;
        }
        Assert.IsTrue(count != 0);
        Assert.AreEqual(fileList.Count, count);
    }

    [TestMethod]
    public void ParamsConstructor()
    {
        var fs = new FileSearch("*");

        using var tmpDir = GetPopulatedTempDir(nameof(DefaultConstructor), out var dirs);
        Assert.IsNotNull(dirs);
        Assert.IsTrue(dirs.Count > 0);

        var filesOracle = AddFiles(tmpDir.DirectoryInfo);

        var count = 0;
        foreach(var fqpn in fs.GetFiles(dirs.ToArray()))
        {
            Assert.IsTrue(filesOracle.Contains(fqpn));
            count++;
        }

        Assert.IsTrue(count != 0);
        Assert.AreEqual(filesOracle.Count, count);
    }

    [TestMethod]
    public void GetFilesIEnumerable()
    {
        var fs = new FileSearch("*");

        using var tmpDir = GetPopulatedTempDir(nameof(DefaultConstructor), out var dirs);
        Assert.IsNotNull(dirs);
        Assert.IsTrue(dirs.Count > 0);

        var filesOracle = AddFiles(tmpDir.DirectoryInfo);

        var count = 0;
        foreach (var fqpn in fs.GetFiles(dirs))
        {
            Assert.IsTrue(filesOracle.Contains(fqpn));
            count++;
        }

        Assert.IsTrue(count != 0);
        Assert.AreEqual(filesOracle.Count, count);
    }
}