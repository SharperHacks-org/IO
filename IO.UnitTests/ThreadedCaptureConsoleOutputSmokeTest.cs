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

#if false // ToDo: These should be run in a seperate process from the other tests.
          // Each can be run successfully, manually, so some kind of synchronization
          // will be required.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharperHacks.IO.UnitTests;

[TestClass]
public class ThreadedCaptureConsoleOutputSmokeTest
{
    private readonly object _lock = new();

    private static void ThreadEntry()
    {
        var outer1 = "Outer1.";
        var outer2 = "Outer2.";
        var inner1 = "Inner1.";
        var inner2 = "Inner2.";

        using var capturedOuter = new CaptureConsoleOutput(100);

        Console.WriteLine(outer1);
        Console.WriteLine(outer2);
        Console.Out.Flush();

        using (var capturedInner = new CaptureConsoleOutput(100000))
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

    [TestMethod]
    public void SmokeThreaded()
    {
        var threads = new List<Thread>();
        Assert.IsNotNull(threads);

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
    }

    // Not clear why we can't run this test case with the SmokeThreaded case,
    // we need to run this to get 100% coverage.
    // TODO: Remove the above comment when the thread test is moved to a new class.
    [TestMethod]
    public void FinalizerWithoutDispose()
    {
        lock (_lock)
        {
            var captured = new CaptureConsoleOutput();
            Assert.IsNotNull(captured);
            var result = GC.GetGeneration(captured);
            GC.Collect(result, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }
    }
}
#endif