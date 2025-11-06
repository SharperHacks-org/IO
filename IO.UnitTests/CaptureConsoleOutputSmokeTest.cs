// Copyright and trademark notices at the end of this file.

using System.Diagnostics.CodeAnalysis;

using SharperHacks.CoreLibs.Reflection;

namespace SharperHacks.CoreLibs.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class CaptureConsoleOutputSmokeTest
{
    private const string _testConsoleOuputString = "This should show up on console output.";

    private readonly object _lock = new();

    private TextWriter _previousConsoleOut = Console.Out;

    [TestInitialize]
    public void Init() => _previousConsoleOut = Console.Out;

    [TestCleanup]
    public void Cleanup() => Console.SetOut(_previousConsoleOut);

    [TestMethod]
    public void SmokeIt()
    {
        lock (_lock)
        {
            var swOut = new StringWriter();
            var swErr = new StringWriter();
            var previousOut = Console.Out;
            var previousError = Console.Error;

            Console.SetOut(swOut);
            Console.SetError(swErr);

            try
            {
                var line1 = "Line 1.";
                var line2 = "Line 2.";
                var previousString = "Previous string.";

                using (var captured = new CaptureConsoleOutput())
                {
                    Console.WriteLine(line1);
                    Console.WriteLine(line2);
                    Assert.Contains(line1, captured.StdOut, Code.AtLineNumber());
                    Assert.Contains(line2, captured.StdOut, Code.AtLineNumber());

                    captured.PreviousStdOut.WriteLine(previousString);
                    Assert.Contains(previousString, swOut.ToString(), Code.AtLineNumber());
                }

                const string inbetweenString = "Inbetween redirects";

                Console.WriteLine(inbetweenString);
                Assert.Contains(inbetweenString, swOut.ToString());

                using (var captured = new CaptureConsoleOutput(3000))
                {
                    Console.WriteLine(line1);
                    Console.WriteLine(line2);
                    Assert.Contains(line1, captured.StdOut, Code.AtLineNumber());
                    Assert.Contains(line2, captured.StdOut, Code.AtLineNumber());
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                var done = "Done?";
                Console.WriteLine(done);
                Assert.Contains(done, swOut.ToString(), Code.AtLineNumber());
                Console.SetOut(previousOut);
                Console.SetError(previousError);
            }
            Console.WriteLine(_testConsoleOuputString);
        }
    }

    private static bool _timeOutExceptionCaught;

    private static void ThisThreadWillCatchTimeoutException()
    {
        try
        {
            using var captured = new CaptureConsoleOutput(10);

            Assert.Fail("We should never get here!");
        }
        catch (TimeoutException)
        {
            _timeOutExceptionCaught = true;
        }
        Assert.IsTrue(_timeOutExceptionCaught);
    }

    [TestMethod]
    public void CaptureConsoleOutputStringTimeSpanThrowsTimeoutException()
    {
        var line1 = "Line 1.";
        var line2 = "Line 2.";

        lock (_lock)
        {
            using var captured = new CaptureConsoleOutput();

            Console.WriteLine(line1);
            Console.WriteLine(line2);
            Assert.Contains(line1, captured.StdOut, Code.AtLineNumber());
            Assert.Contains(line2, captured.StdOut, Code.AtLineNumber());

            var thread = new Thread(ThisThreadWillCatchTimeoutException);
            thread.Start();

            while (thread.ThreadState != ThreadState.Stopped)
            {
                Thread.Sleep(50);
            }
        }
        Assert.IsTrue(_timeOutExceptionCaught, Code.AtLineNumber());

        Console.WriteLine(_testConsoleOuputString);
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
