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

namespace SharperHacks.IO;

/// <summary>
/// Interactive class wraps some common user I/O interactions.
/// </summary>
public class Interactive
{
    /// <summary>
    /// The input stream to use.
    /// </summary>
    [NotNull] public TextReader InReader {  get; set; } = Console.In;

    /// <summary>
    /// The output stream to use.
    /// </summary>
    [NotNull] public TextWriter OutWriter {  get; set; } = Console.Out;

    /// <summary>
    /// Any prompt prefix string to prepend to user prompts.
    /// </summary>
    [NotNull] public string PromptPrefix {  get; set; } = string.Empty;

    // TODO: Get rid of these. Caller should provide them.

    /// <summary>
    /// Yes/no prefix prompt.
    /// </summary>
    [NotNull] public string YesNoPostfix {  get; set; } = " <y|n>? ";

    /// <summary>
    /// Valid yes responses.
    /// </summary>
    [NotNull] public string[] ValidYesResponses { get; set; } = _validYesResponses;
    private static string[] _validYesResponses = { "y", "yes" };

    /// <summary>
    /// Valid no responses.
    /// </summary>
    [NotNull] public string[] ValidNoResponses { get; set; } = _validNoResponses;
    private static string[] _validNoResponses = { "n", "no" };

    /// <summary>
    /// Invalid response prompt.
    /// </summary>
    [NotNull]
    public string InvalidResponse { get; set; } = "Invalid response. Try again (Ctrl+C to exit).";

    /// <summary>
    /// Get a response string from the user.
    /// </summary>
    /// <param name="question">The prompt to illicit user response.</param>
    /// <returns></returns>
    public string GetString(string question)
    {
        OutWriter.Write(PromptPrefix + question);
        var result = InReader.ReadLine() ?? string.Empty;

        return result;
    }

    /// <summary>
    /// Illicit a yes not response from the user.
    /// </summary>
    /// <param name="question"></param>
    /// <param name="suppressYesNoPostfix"></param>
    /// <returns></returns>
    public bool GetYesNo(string question, bool suppressYesNoPostfix = false)
    {
        var result = false;
        // TODO: Add max retry?
        while (true)
        {
            var response = GetString(question + (suppressYesNoPostfix ? string.Empty : YesNoPostfix));

            if (ValidYesResponses.Contains(response))
            {
                result = true;

                break;
            }
            if (ValidNoResponses.Contains(response)) break;

            // else
            OutWriter.WriteLine(InvalidResponse);
        }

        return result;
    }
}