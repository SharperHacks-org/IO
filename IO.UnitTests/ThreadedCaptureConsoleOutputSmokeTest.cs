// Copyright and trademark notices at the end of this file.

#if true

// ToDo: Each of these test cases has to be run manually.
// Investigate why MsTest will not run both of these with the rest of the tests.
// 

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Diagnostics.CodeAnalysis;

namespace SharperHacks.CoreLibs.IO.UnitTests;

[ExcludeFromCodeCoverage]
[TestClass]
public class ThreadedCaptureConsoleOutputSmokeTest
{
    // This lock is used to prevent test cases from running in parallel.
#if NET9_0_OR_GREATER
    private static readonly Lock _lock = new();
#else
    private static readonly object _lock = new();
#endif

    private static void ThreadEntry()
    {
        var outer1 = Environment.CurrentManagedThreadId.ToString();//"Outer1.";
        var outer2 = "Outer2.";
        var inner1 = "Inner1.";
        var inner2 = "Inner2.";

        using var capturedOuter = new CaptureConsoleOutput(1000);

        Console.WriteLine(outer1);
        Console.WriteLine(outer2);
        Console.Out.Flush();

        using (var capturedInner = new CaptureConsoleOutput())
        {
            Console.WriteLine(inner1);
            Console.WriteLine(inner2);
            Console.Out.Flush();

            Assert.IsTrue(capturedInner.CapturedOutput.Contains(inner1));
            Assert.IsTrue(capturedInner.CapturedOutput.Contains(inner2));
            Assert.IsFalse(capturedInner.CapturedOutput.Contains(outer1));
            Assert.IsFalse(capturedInner.CapturedOutput.Contains(outer2));
        }

        Assert.IsTrue(capturedOuter.CapturedOutput.Contains(outer1));
        Assert.IsTrue(capturedOuter.CapturedOutput.Contains(outer2));
        Assert.IsFalse(capturedOuter.CapturedOutput.Contains(inner1));
        Assert.IsFalse(capturedOuter.CapturedOutput.Contains(inner2));
    }

    // When this test is run manually, it always succeeds.
    [TestMethod]
    public void SmokeThreaded()
    {
        var threads = new List<Thread>();
        Assert.IsNotNull(threads);

        Console.WriteLine("We run!");
        Console.WriteLine(DateTime.Now.ToString());

        lock (_lock)
        {
            for (var count = 0; count < 20; count++)
            {
                threads.Add(new Thread(ThreadEntry));
            }

            foreach (var thread in threads)
            {
                Assert.IsNotNull(thread);
                thread.Start();
                Thread.Sleep(1);
            }

            foreach (var thread in threads)
            {
                thread.Join(50000);
            }
        }

        Console.WriteLine("We always see this.");
    }
}
#endif

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
