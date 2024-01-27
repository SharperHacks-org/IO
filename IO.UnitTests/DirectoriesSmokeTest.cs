// Copyright and trademark notices at the end of this file.

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Diagnostics.CodeAnalysis;

namespace SharperHacks.CoreLibs.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class DirectoriesSmokeTest //: TestBase
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

    private HashSet<string> GetPathOracle(string root)
    {
        var result = new HashSet<string>();

        foreach (var subDir in _subDirs)
        {
            _ = result.Add(Path.Join(root, subDir));
        }

        return result;
    }

    private TempDirectory GetPopulatedTempDir(string prefix)
    {
        var tmpDir = new TempDirectory($"{nameof(DirectoriesSmokeTest)}.{prefix}");

        foreach (var subDir in _subDirs)
        {
            _ = Directory.CreateDirectory(Path.Join(tmpDir.DirectoryInfo.FullName, subDir));
        }

        return tmpDir;
    }

    [TestMethod]
    public void DefaultConstructorTests()
    {
        var dirs = new Directories();
        
        Assert.AreEqual(SearchOption.AllDirectories, dirs.SearchOption);
        Assert.IsNotNull(dirs.Roots);
        Assert.AreEqual(1, dirs.Roots.Count());
        Assert.AreEqual(".", dirs.Roots.First());

        // We don't bother to iterate the whole tree from here, as it's the
        // current working directory of the test host. This test just covers
        // the dirs state and constructor code. The rest of the class code will
        // be covered elsewhere.
    }

    [TestMethod]
    public void ParamsRootConstructor()
    {
        // First verify the default (empty params list) behavior...
        var dirs = new Directories(SearchOption.TopDirectoryOnly);
        Assert.AreEqual(SearchOption.TopDirectoryOnly, dirs.SearchOption);
        Assert.IsNotNull(dirs.Roots);
        Assert.AreEqual(1, dirs.Roots.Count());
        Assert.AreEqual(".", dirs.Roots.First());

        // Now do something interesting...
        using var tmpDir = GetPopulatedTempDir(nameof(ParamsRootConstructor));
        var oracle = GetPathOracle(tmpDir.DirectoryInfo.FullName);

        dirs = new Directories(SearchOption.AllDirectories, tmpDir.DirectoryInfo.FullName);

        var count = 0;
        foreach (var dir in dirs.GetDirectories())
        {
            Assert.IsTrue(oracle.Contains(dir));
            count++;
        }
        Assert.AreEqual(_subDirs.Length, count);
    }

    [TestMethod]
    public void IEnumerableRootConstructor()
    {
        using var tmpDir = GetPopulatedTempDir(nameof(ParamsRootConstructor));
        var oracle = GetPathOracle(tmpDir.DirectoryInfo.FullName);

        var dirs = new Directories(SearchOption.TopDirectoryOnly, oracle);
        
        var count = 0;
        foreach (var dir in dirs.GetDirectories())
        {
            Assert.IsTrue(oracle.Contains(dir));
            count++;
        }
        Assert.AreEqual(6, count);
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
