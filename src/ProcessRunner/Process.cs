using System.IO;
using System.Diagnostics;

namespace ProcessRunner
{
    /// <summary>
    /// Runs external processes
    /// </summary>
    public class Process
    {
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
    }
}
