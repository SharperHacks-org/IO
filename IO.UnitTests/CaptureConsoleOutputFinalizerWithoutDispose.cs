// Copyright and trademark notices at the end of this file.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharperHacks.CoreLibs.IO.UnitTests;

// This test always causes the ThreadedCaptureOutputSmokeTest to fail with timeout exceptions.

[TestClass]
[Ignore]
public class CaptureConsoleOutputFinalizerWithoutDispose
{
    [TestMethod]
    public void FinalizerWithoutDispose()
    {
        Console.WriteLine("We run!");

        var captured = new CaptureConsoleOutput();
        Assert.IsNotNull(captured);
        var result = GC.GetGeneration(captured);
        GC.Collect(result, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();

        // It's probably a bug, but this is definitely a corner case.
        Console.WriteLine("???BUG??? We never see this in the test console.");
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

