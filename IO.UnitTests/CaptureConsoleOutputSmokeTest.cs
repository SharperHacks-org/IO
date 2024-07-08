// Copyright and trademark notices at the end of this file.

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharperHacks.CoreLibs.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class CaptureConsoleOutputSmokeTest
{
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
            var sw = new StringWriter();
            var previousOut = Console.Out;
            Console.SetOut(sw);

            try
            {
                var line1 = "Line 1.";
                var line2 = "Line 2.";
                var previousString = "Previous string.";

                using (var captured = new CaptureConsoleOutput())
                {
                    Console.WriteLine(line1);
                    Console.WriteLine(line2);
                    Assert.IsTrue(captured.CapturedOutput.Contains(line1));
                    Assert.IsTrue(captured.CapturedOutput.Contains(line2));

                    captured.PreviousWriter.WriteLine(previousString);
                    Assert.IsTrue(sw.ToString().Contains(previousString));
                }

                const string inbetweenString = "Inbetween redirects";

                Console.WriteLine(inbetweenString);
                Assert.IsTrue(sw.ToString().Contains(inbetweenString));

                using (var captured = new CaptureConsoleOutput(3000))
                {
                    Console.WriteLine(line1);
                    Console.WriteLine(line2);
                    Assert.IsTrue(captured.CapturedOutput.Contains(line1));
                    Assert.IsTrue(captured.CapturedOutput.Contains(line2));
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
                Assert.IsTrue(sw.ToString().Contains(done));
                Console.SetOut(previousOut);
            }
        }
    }

#if false // TODO: Try moving this to a separate test class and running it there.
#endif

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
            Assert.IsTrue(captured.CapturedOutput.Contains(line1));
            Assert.IsTrue(captured.CapturedOutput.Contains(line2));

            var thread = new Thread(ThisThreadWillCatchTimeoutException);
            thread.Start();

            while (thread.ThreadState != ThreadState.Stopped)
            {
                Thread.Sleep(50);
            }
        }
        Assert.IsTrue(_timeOutExceptionCaught);
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
