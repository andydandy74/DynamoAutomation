using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Dynamo.Automation
{
    /// <summary>
    /// Automates Journal creation.
    /// </summary>
    public class Journal
    {
        private Journal() { }
        /// <summary>
        /// Create a journal file for executing a Dynamo workspace on a Revit file.
        /// 
        /// This journal file uses several keys specifically for the purpose of automating Dynamo.
        /// Dynamo is never run in the idle loop. The workspace is executed immediately, and control is returned to the DynamoRevit
        /// external application.
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file.</param>
        /// <param name="workspacePath">The path to the Dynamo workspace.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <returns>The path of the generated journal file.</returns>
        public static string ByWorkspacePath(string revitFilePath, string workspacePath, string journalFilePath, int revitVersion)
        {
            // Exception if the dyn file isn't found
            if (!File.Exists(workspacePath))
            {
                throw new FileNotFoundException();
            }

            // Delete journal file if it already exists
            if (File.Exists(journalFilePath))
            {
                File.Delete(journalFilePath);
            }

            // Finding the installed version of Dynamo Revit
            // Successfully stole this part from Dynamo StartupUtils.cs and modified it as needed :-)
            const string regKey64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            // Open HKLM for 64bit registry
            var regKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            // Open Windows/CurrentVersion/Uninstall registry key
            regKey = regKey.OpenSubKey(regKey64);
            // Get "Version" value as string for all the subkeys that start with "Dynamo Revit"
            var installedVersions = regKey.GetSubKeyNames().Where(s => s.StartsWith("Dynamo Revit")).Select((s) => regKey.OpenSubKey(s).GetValue("Version") as string);
            string dynVersion = installedVersions.First().Substring(0, 3);
            // This code is not bothering to check if any pre-1.0 Dynamo is installed - lazy...

            // Launch command to be used depends on Revit & Dynamo versions
            string launchCommand;
            string launchArgs;
            // Launch command and journal keys differ between pre-2017 and post-2016 Revit
            if (revitVersion > 2016)
            {
                launchCommand = "Jrn.Command \"Ribbon\" , \"Launch Dynamo, ID_VISUAL_PROGRAMMING_DYNAMO\" \n";
                launchArgs = String.Format("Jrn.Data \"APIStringStringMapJournalData\", 5, \"dynPath\", \"{0}\", \"dynShowUI\", \"true\", \"dynAutomation\", \"false\", \"dynPathExecute\", \"true\", \"dynModelShutDown\", \"true\" \n", workspacePath.Replace(' ', '/'));
            }
            else
            {
                launchCommand = String.Format("Jrn.RibbonEvent \"Execute external command:CustomCtrl_%CustomCtrl_%Add-Ins%Visual Programming%Dynamo {0}:Dynamo.Applications.DynamoRevit\" \n", dynVersion);
                launchArgs = String.Format("Jrn.Data \"APIStringStringMapJournalData\", 3, \"dynPath\", \"{0}\", \"dynShowUI\", \"false\", \"dynAutomation\", \"true\" \n", workspacePath.Replace(' ', '/'));
            }

            // Create journal file
            using (var tw = new StreamWriter(journalFilePath, true))
            {
                var journal = String.Format(@"'" +
                                            "Dim Jrn \n" +
                                            "Set Jrn = CrsJournalScript \n" +
                                            "Jrn.Command \"StartupPage\" , \"Open this project , ID_FILE_MRU_FIRST\" \n" +
                                            "Jrn.Data \"MRUFileName\"  , \"{0}\" \n" +
                                            "{1}" +
                                            "{2}" +
                                            "Jrn.Command \"Internal\" , \"Flush undo and redo stacks , ID_FLUSH_UNDO\" \n" +
                                            "Jrn.Command \"Internal\" , \"Close the active project , ID_REVIT_FILE_CLOSE\" \n" +
                                            "Jrn.Command \"SystemMenu\" , \"Quit the application; prompts to save projects , ID_APP_EXIT\"",
                    revitFilePath.Replace(' ', '/'), launchCommand,  launchArgs);
                tw.Write(journal);
                tw.Flush();
            }

            return journalFilePath;
        }
    }
}
