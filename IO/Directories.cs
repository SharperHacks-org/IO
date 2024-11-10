// Copyright and trademark notices at the end of this file.

namespace SharperHacks.CoreLibs.IO;

// TODO: Figure out how to improve, optimize and asynchronize this, when .NET has proper glob star support.
// TODO: Add a cache?

/// <summary>
/// A class for enumerating a set of root directories and their subdirectories.
/// </summary>
public class Directories
{
    #region Public

    /// <summary>
    /// An enumeration of the configured root paths.
    /// </summary>
    public IEnumerable<string> Roots { get; }

    /// <summary>
    /// Whether to recurse just the configured roots, or also their subdirectories.
    /// </summary>
    public SearchOption SearchOption { get; }

    /// <summary>
    /// Iterates over the directories found.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public IEnumerable<string> GetDirectories(string pattern = "*")
    {
        foreach (var targetDir in Roots)
        {
            foreach (var dir in GetDirectories(targetDir, pattern))
            {
                yield return dir;
            }
        }
    }

    #region Constructors

    /// <summary>
    /// Default constructor. Sets <see cref="Roots"/> equal to "." and
    /// <see cref="SearchOption"/> equal to SearchOption.AllDirectories
    /// </summary>
    public Directories()
    {
        Roots = new[] {"."};
        SearchOption = SearchOption.AllDirectories;
    }

    /// <summary>
    /// Constructor taking SearchOption and IEnumerable of the root paths.
    /// </summary>
    /// <param name="option"></param>
    /// <param name="roots"></param>
    public Directories(SearchOption option, IEnumerable<string> roots)
    {
        var enumerable = roots as string[] ?? roots.ToArray();
        Roots = enumerable.Length > 0 ? enumerable : ["."];
        SearchOption = option;
    }

    /// <summary>
    /// Variadic constructor taking SearchOption and zero or more root paths.
    /// </summary>
    /// <param name="option"></param>
    /// <param name="roots"></param>
    public Directories(SearchOption option, params string[] roots)
    {
        Roots = roots.Length > 0 ? roots : ["."];
        SearchOption = option;
    }

    #endregion Constructors

    #endregion Public

    #region Private

    private IEnumerable<string> GetDirectories(string root, string pattern)
    {
        if (SearchOption == SearchOption.AllDirectories)
        {
            foreach (var dir in Directory.EnumerateDirectories(root, pattern, SearchOption))
            {
                yield return Path.GetFullPath(dir);
            }
        }
        else
        {
            yield return Path.GetFullPath(root);
        }
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
