// Copyright and trademark notices at the end of this file.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualStudio.TestPlatform.Common.Exceptions;



//using JetBrains.Annotations;

//using Serilog;

using SharperHacks.CoreLibs.Constraints;
//using SharperHacks.Diagnostics.Logger;

// WIP: This class is incomplete in design and implementation.

namespace SharperHacks.Diagnostics.OS
{
    /// <summary>
    /// Wrapper class for running separate processes, and capturing their output.
    /// </summary>
    public class ShellExec
    {
        #region public

        /// <summary>
        /// The args to run Cmd with.
        /// </summary>
        [NotNull] public string Args { get; set; }

        /// <summary>
        /// The command to execute.
        /// </summary>
        [NotNull] public string Cmd { get; set; }

        /// <summary>
        /// The ProcessStartInfo used to execute the command.
        /// </summary>
        /// <remarks>
        /// Initialized by the constructors. Can be modified before calling
        /// RunSync() or RunAsync().
        /// </remarks>
        public ProcessStartInfo ProcessStartInfo { get; set; }

        /// <summary>
        /// The Process object used to execute the command.
        /// </summary>
        /// <remarks>
        /// Initialized by the constructors. Can be modified before calling
        /// RunSync() or RunAsync().
        /// </remarks>
        [NotNull] public Process Process { get; set; }

        /// <summary>
        /// The captured output from the last command execution.
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// Run Cmd with Args, synchronously.
        /// </summary>
        /// <returns></returns>
        public int RunSync()
        {
//            _log.TraceEntry();

            _ = Process.Start();
            Result = Process.StandardOutput.ReadToEnd();

//            _log.TraceExit();

            return Process.ExitCode;
        }
        
        #region Constructors

        /// <summary>
        /// Simple constructor, uses default ProcessStartInfo settings and adds cmd and args.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        public ShellExec(string cmd, string args)
        {
//            _log.TraceEntry();

            Verify.IsNotNull(cmd);
            Verify.IsNotNull(args);

            Cmd = cmd;
            Args = args;

            var psi = new ProcessStartInfo(Cmd, Args)
            {
                RedirectStandardOutput = true, // Capture output.
                UseShellExecute = false, // No graphical shell.
                CreateNoWindow = true,
                FileName = Cmd,
                Arguments = Args // Execute in background (no window).
            };

            ProcessStartInfo = psi;

            Process = new Process
            {
                StartInfo = ProcessStartInfo
            };

            Result = string.Empty;

//            _log.TraceExit();
        }

        /// <summary>
        /// Construct an instance from an initialized ProcessStartInfo object.
        /// </summary>
        /// <param name="psi"></param>
        public ShellExec(ProcessStartInfo psi)
        {
//            _log.TraceEntry();

            Verify.IsNotNull(psi);

            Cmd = psi.FileName;
            Args = psi.Arguments;
            ProcessStartInfo = psi;

            Process = new Process
            {
                StartInfo = ProcessStartInfo
            };

            Result = string.Empty;

//            _log.TraceExit();
        }

        #endregion Constructors

        #endregion public

        #region private

//        private static readonly ILogger _log = Log.ForContext<ShellExec>() ?? Verify.ThrowValueIsNullException<ILogger>("_log is null.");

        #endregion private
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
