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

using SharperHacks.Constraints;

using System.Globalization;

namespace SharperHacks.IO;

/// <summary>
/// Miscellaneous file related helper functions used by some SharperHacks.IO classes.
/// </summary>
public static class FileHelpers
{
    /// <summary>
    /// Message employed when a pattern string contains an invalid specifier.
    /// </summary>
    public static readonly string InvalidSpecifierExceptionMsg = "Invalid specifier.";

    private static readonly IFormatProvider _defaultFormatProvider = CultureInfo.InvariantCulture;

    /// <summary>
    /// Find the value of the highest numbered file matching the supplied pattern.
    /// </summary>
    /// <param name="pattern">
    /// Prefix{n|t}Postfix, where {n|Tn} marks the position in the number or timestamp in the file name.
    /// </param>
    /// <returns></returns>
    /// <remarks>
    /// The specifier may be located anywhere in <paramref name="pattern"/>, provided it is not part of
    /// any directory path content in the prefix part. The specifier must be somewhere in the file name or extension.
    /// </remarks>
    public static long HighestN(string pattern)
    {
        Verify.IsNotNullOrEmpty(pattern);

        var (prefix, specifier, postfix) = SplitPattern(pattern);

        // ReSharper code coverage misses the default case below, but not above.
        var result = specifier switch
        {
            "n" => HighestN(prefix, postfix),
            //"d" => HighestDateTimestamp(prefix, postfix),
            _ => throw new ArgumentException(InvalidSpecifierExceptionMsg, nameof(pattern))
        };
        return result;
    }

    /// <summary>
    /// Find the numeric value of the highest numbered file matching the prefix and postfix.
    /// </summary>
    /// <param name="prefix">Optional path and file name prefix.</param>
    /// <param name="postfix">Optional file name postfix, including any extension.</param>
    /// <returns>The highest value found.</returns>
    /// <remarks>
    /// <para>
    /// Searches for prefix#postfix, where # can be any number of the digits 0..9.
    /// </para>
    /// </remarks>
    public static long HighestN(string? prefix, string? postfix)
    {
        var (path, fileNamePrefix) = SplitPathFromFileNamePrefix(prefix ?? ".");

#pragma warning disable CA1305 // Specify IFormatProvider
        var result = HighestN(
                path ?? string.Empty,
                fileNamePrefix ?? string.Empty,
                postfix ?? string.Empty);
#pragma warning restore CA1305 // Specify IFormatProvider

        return result;
    }

    /// <summary>
    /// Find the numeric value of the highest numbered file in path, matching the prefix and postfix.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileNamePrefix"></param>
    /// <param name="postfix"></param>
    /// <param name="style"></param>
    /// <param name="provider"></param>
    /// <returns>Highest value found.</returns>
    public static long HighestN(
            string path, 
            string fileNamePrefix, 
            string postfix,
            NumberStyles style = NumberStyles.None,
            IFormatProvider? provider = null
            )
    {
        long highestNum = -1;

        // ReSharper disable once PossibleNullReferenceException
        foreach (var item in Directory.EnumerateFiles(path, fileNamePrefix + "*" + postfix))
        {
            // item can be a string of the form path + non-numeric + postfix,
            // so we have to filter those out, while tracking the highest number
            // we can find.

            var middle = item![(path.Length + fileNamePrefix.Length)..^postfix.Length];

            if (string.IsNullOrEmpty(middle)) continue;

            var isAllNumeric = true;

            foreach (var c in middle)
            {
                if (!char.IsDigit(c))
                {
                    isAllNumeric = false; // Noise in the middle.

                    break;
                }
            }

            if (!isAllNumeric) continue;

            if (long.TryParse(middle, style, provider ?? _defaultFormatProvider, out var num))
            {
                if (num > highestNum) highestNum = num;
            }
        }

        return highestNum;
    }

    /// <summary>
    /// Splits the supplied pattern into prefix, specifier and postfix.
    /// </summary>
    /// <param name="pattern"><see cref="HighestN(string)"/></param>
    /// <returns>(string prefix, string specifier, string postfix)</returns>
    /// <remarks>
    /// This helper written for the HighestN methods but there's no reason to hide it
    /// and it is easier for unit tests to test all code paths when it is public.
    /// </remarks>
    public static (string prefix, string specifier, string postfix) SplitPattern(string pattern)
    {
        // Note: Regex is slow and non-deterministic, so we avoid it here.
        Verify.IsNotNullOrEmpty(pattern);

        var prefix = string.Empty;
        var postfix = string.Empty; 
        var startIdx = 0;

        // Get the prefix, if any...
        var currentIdx = pattern.IndexOf('{');
        
        if (currentIdx == -1) throw new ArgumentException("Missing specifier.", nameof(pattern));

        if (currentIdx > 0)
        {
            prefix = pattern[startIdx..currentIdx];
            startIdx = currentIdx + 1;
        }
        else
        {
            // prefix is left empty.
            startIdx = 1;
        }

        // Get the specifier if any...
        currentIdx = pattern.IndexOf('}');

        if (currentIdx <= startIdx) throw new ArgumentException("Malformed specifier", nameof(pattern));

        var specifier = pattern[startIdx..currentIdx];
        startIdx = currentIdx + 1;

        // Get the postfix if any...
        if (startIdx < pattern.Length - 1) postfix = pattern[startIdx..];

        return (prefix, specifier, postfix);
    }

    /// <summary>
    /// Splits the path from the file name prefix.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    /// <remarks>Public for unit testing.</remarks>
    public static (string path, string fileNamePrefix) SplitPathFromFileNamePrefix(string prefix)
    {
        var idxOfLastPathSeparator = prefix.LastIndexOf(Path.DirectorySeparatorChar);

        if (-1 == idxOfLastPathSeparator)
        {
            idxOfLastPathSeparator = prefix.LastIndexOf(Path.AltDirectorySeparatorChar);
        }

        if (-1 == idxOfLastPathSeparator) return (".", prefix);

        var path = Path.GetFullPath(prefix)[..(idxOfLastPathSeparator + 1)];

        return path!.Length == prefix.Length ? (path, string.Empty) : (path, prefix[(idxOfLastPathSeparator + 1)..]);
    }
}