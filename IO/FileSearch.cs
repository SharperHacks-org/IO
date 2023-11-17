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


using SharperHacks.Constraints;

namespace SharperHacks.IO;

/// <summary>
/// File search pattern container and file enumerators.
/// </summary>
public class FileSearch
{
    #region Public

    /// <summary>
    /// An IEnumerable of the configured patterns to use.
    /// </summary>
    public IEnumerable<string> Patterns { get; }

    #region Constructors

    /// <summary>
    /// Default Constructor.
    /// </summary>
    /// <param name="patterns">"*" is used if this empty.</param>
    public FileSearch(params string[] patterns)
    {
        Patterns = Array.Empty<string>() == patterns ? (new[] { "*" }) : patterns;
    }

    #endregion Constructos

    /// <summary>
    /// Get the files that match Pattern. 
    /// </summary>
    /// <param name="dirs"></param>
    /// <returns></returns>
    public IEnumerable<string> GetFiles(params string[] dirs)
    {
        Verify.IsNotNull(dirs);

        if (Array.Empty<string>() == dirs)
        {
            dirs = new[] { Directory.GetCurrentDirectory() };
        }

        foreach (var dir in dirs)
        {
            Verify.IsNotNull(dir);
            foreach (var pattern in Patterns)
            {
                foreach (var fileName in Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly))
                {
                    yield return fileName;
                }
            }
        }
    }

    /// <summary>
    /// Get the files that match Pattern.
    /// </summary>
    /// <param name="dirs"></param>
    /// <returns></returns>
    public IEnumerable<string> GetFiles(IEnumerable<string> dirs)
    {
        Verify.IsNotNull(dirs);

        foreach (var dir in dirs)
        {
            foreach (var pattern in Patterns)
            {
                foreach (var fqpn in Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly))
                {
                    yield return fqpn;
                }
            }
        }
    }

    #endregion Public
}