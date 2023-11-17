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

using System.Text;

using SharperHacks.Constraints;

namespace SharperHacks.IO;

/// <summary>
/// Provides static numbered file creation method.
/// </summary>
public static class NumberedFile
{
    /// <summary>
    /// Create a file of the form {path}\{name}{sep}#.{ext}
    /// </summary>
    /// <param name="path">Directory path. Current directory used if null.</param>
    /// <param name="name">File name prefix. Ignored if null.</param>
    /// <param name="ext">Extension without the leading period. Ignored if null.</param>
    /// <param name="sep">String to separate name part from number part. Default "-".</param>
    /// <param name="floor">Lowest number to try. Default 1.</param>
    /// <returns>FileStream</returns>
    public static FileStream IntegerAutoIncrementCreate(
            string? path = null, 
            string? name = null, 
            string? ext = null,
            string? sep = null,
            long floor = 1
            )
    {
        FileStream? result = null;

//        string prefix = Path.Combine(path ?? string.Empty, name ?? string.Empty)
//        long highestCurrentNumber = FileHelpers.HighestN(path, name + sep, '.' + ext);

        var pathFileName = BuildPathFileName(path, name, sep, floor, ext);
        Verify.IsNotNullOrEmpty(pathFileName);

        do
        {
            try
            {
                result = new FileStream(pathFileName, FileMode.CreateNew, FileAccess.ReadWrite);
            }
            catch (IOException)
            {
                pathFileName = BuildPathFileName(path, name, sep, ++floor, ext);
            }
        } while (null == result);

        return result;
    }

    private static string BuildPathFileName(
            string? path,
            string? name,
            string? sep,
            long num,
            string? ext
            )
    {
        var sb = new StringBuilder();
        _ = sb.Append(Path.Combine(path ?? string.Empty, name ?? string.Empty))
              .Append(sep ?? string.Empty)
              .Append(num);
        if (null != ext) _ = sb.Append('.').Append(ext);

        var result = sb.ToString();

        return result;
    }
}