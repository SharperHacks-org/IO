// Copyright and trademark notices at the end of this file.

using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharperHacks.CoreLibs.IO.UnitTests;

[ExcludeFromCodeCoverage()]
[TestClass]
public class InteractiveSmokeTests //: TestBase
{
    [ExcludeFromCodeCoverage]
    [TestMethod]
    public void GetStringSmokeTest()
    {
        var promptString = "Tell me something: ";
        var responseString = "Ya, whatever.";

        using var spew = new CaptureConsoleOutput();
        Assert.IsNotNull(spew);

        // GetString() performs a synchronous read on Console.In,
        // so we have to stuff the input queue first...
        var previousStringReader = Console.In;
        var stringReader = new StringReader(responseString + '\n');
        Console.SetIn(stringReader);

        var conio = new Interactive();
        var inputString = conio.GetString(promptString);

        Console.SetIn(previousStringReader);

        Assert.Contains(responseString, inputString);
        Assert.DoesNotContain(promptString, inputString);
        Assert.Contains(promptString, spew.StdOut);
    }

    [TestMethod]
    public void GetYesNoSmokeTest()
    {
        var previousStringReader = Console.In;

        try
        {
            var conio = new Interactive();

            foreach (var response in conio.ValidYesResponses)
            {
                Assert.IsTrue(GetYesNoResult(response, conio));
            }

            foreach (var response in conio.ValidNoResponses)
            {
                Assert.IsFalse(GetYesNoResult(response, conio));
            }

            // GetYesNo should issue an invalid response string and re-prompt for input on invalid entries.
            using var spew = new CaptureConsoleOutput();
            conio.OutWriter = Console.Out;
            Assert.IsFalse(GetYesNoResult("blah\nblah\nno\n", conio));
            Assert.Contains(conio.InvalidResponse, spew.StdOut);
        }
        finally
        {
            Console.SetIn(previousStringReader);
        }
    }

    private static bool GetYesNoResult(string response, Interactive conio)
    {
        var promptString = "Is okay";
        var stringReader = new StringReader(response + '\n');
        Console.SetIn(stringReader);
        conio.InReader = Console.In;

        return conio.GetYesNo(promptString);
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
