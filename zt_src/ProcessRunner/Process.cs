using System.IO;
using System.Diagnostics;

namespace ProcessRunner
{
    /// <summary>
    /// Runs external processes
    /// </summary>
    public class Process
    {
        private Process() { }
        /// <summary>
        /// Run a process and return the exit code.
        /// </summary>
        /// <param name="processPath">The path to the process to execute.</param>
        /// <param name="args">The command line arguments to the process.</param>
        /// <returns>The exit code for the process.</returns>
        public static int ByPathAndArguments(string processPath, string args)
        {
            if (!File.Exists(processPath))
            {
                throw new FileNotFoundException();
            }

            var process = new System.Diagnostics.Process {StartInfo = new ProcessStartInfo(processPath, args)};
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        /// <summary>
        /// Kill the current process and return the exit code.
        /// ATTENTION: Use carefully! If run in DynamoSandbox this node will kill the DynamoSandbox.exe process, if run in DynamoRevit it will kill the Revit.exe process!
        /// </summary>
        /// <param name="toggle">Should the current process be terminted?</param>
        /// <returns>The exit code for the process.</returns>
        public static int KillCurrentProcess(bool toggle = false)
        {
            if (toggle)
            {
                System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                currentProcess.Kill();
                return currentProcess.ExitCode;
            }
            else
            {
                return 0;
            }
        }
    }
}
