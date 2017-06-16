using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// Find the current Dynamo version
        /// </summary>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <returns>The first three digits of the active Dynamo version.</returns>
        private static string GetDynamoVersion(int revitVersion)
        {
            string dynVersion;
            if (revitVersion > 2016)
            {
                dynVersion = "";
            }
            else
            {
                // Finding the installed version of Dynamo
                Assembly dynamoCore = Assembly.Load("DynamoCore");
                dynVersion = dynamoCore.GetName().Version.ToString().Substring(0, 3);
            }
            return dynVersion;
        }
        /// <summary>
        /// Builds the first part of the journal string
        /// </summary>
        /// <returns>The first part of the journal string.</returns>
        private static string BuildJournalStart()
        {
            string journalStart = @"'" +
                "Dim Jrn \n" +
                "Set Jrn = CrsJournalScript \n";
            return journalStart;
        }
        /// <summary>
        /// Builds the last part of the journal string
        /// </summary>
        /// <returns>The last part of the journal string.</returns>
        private static string BuildJournalEnd()
        {
            string journalEnd = "Jrn.Command \"SystemMenu\" , \"Quit the application; prompts to save projects , ID_APP_EXIT\"";
            return journalEnd;
        }
        /// <summary>
        /// Builds the part of the journal string responsible for opening a project
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file.</param>
        /// <returns>The part of the journal string responsible for opening a project.</returns>
        private static string BuildProjectOpen(string revitFilePath)
        {
            // Exception if the rvt/rfa file isn't found
            if (!File.Exists(revitFilePath))
            {
                throw new FileNotFoundException();
            }
            string projectOpen = String.Format("Jrn.Command \"StartupPage\" , \"Open this project , ID_FILE_MRU_FIRST\" \n" +
                                            "Jrn.Data \"MRUFileName\"  , \"{0}\" \n", revitFilePath.Replace(' ', '/'));
            return projectOpen;
        }
        /// <summary>
        /// Builds the part of the journal string responsible for closing a project
        /// </summary>
        /// <returns>The part of the journal string responsible for closing a project.</returns>
        private static string BuildProjectClose()
        {
            string projectClose = "Jrn.Command \"Internal\" , \"Close the active project , ID_REVIT_FILE_CLOSE\" \n";
            return projectClose;
        }
        /// <summary>
        /// Builds the part of the journal string responsible for launching DynamoRevit
        /// </summary>
        /// <param name="workspacePath">The path to the Dynamo workspace.</param>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <param name="dynVersion">The version number of Dynamo (e.g. 1.3).</param>
        /// <returns>The part of the journal string responsible for launching DynamoRevit.</returns>
        private static string BuildLaunchDynamo(string workspacePath, int revitVersion, string dynVersion)
        {
            // Exception if the dyn file isn't found
            if (!File.Exists(workspacePath))
            {
                throw new FileNotFoundException();
            }
            string launchDynamo = "";
            // Launch command and journal keys differ between pre-2017 and post-2016 Revit
            if (revitVersion > 2016)
            {
                launchDynamo += String.Format("Jrn.Command \"Ribbon\" , \"Launch Dynamo, ID_VISUAL_PROGRAMMING_DYNAMO\" \n" +
                    "Jrn.Data \"APIStringStringMapJournalData\", 5, \"dynPath\", \"{0}\", \"dynShowUI\", \"false\", \"dynAutomation\", \"false\", \"dynPathExecute\", \"true\", \"dynModelShutDown\", \"true\" \n", workspacePath.Replace(' ', '/'));
            }
            else
            {
                launchDynamo += String.Format("Jrn.RibbonEvent \"Execute external command:CustomCtrl_%CustomCtrl_%Add-Ins%Visual Programming%Dynamo {0}:Dynamo.Applications.DynamoRevit\" \n" +
                    "Jrn.Data \"APIStringStringMapJournalData\", 3, \"dynPath\", \"{1}\", \"dynShowUI\", \"false\", \"dynAutomation\", \"true\" \n", dynVersion, workspacePath.Replace(' ', '/'));
            }
            launchDynamo += "Jrn.Command \"Internal\" , \"Flush undo and redo stacks , ID_FLUSH_UNDO\" \n";
            return launchDynamo;
        }
        /// <summary>
        /// Deletes the journal file if it already exists.
        /// </summary>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        private static void DeleteJournalFile(string journalFilePath)
        {
            if (File.Exists(journalFilePath))
            {
                File.Delete(journalFilePath);
            }
            return;
        }
        /// <summary>
        /// Writes the journal file.
        /// </summary>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <param name="journalString">The string for the journal file.</param>
        private static void WriteJournalFile(string journalFilePath, string journalString)
        {
            var tw = new StreamWriter(journalFilePath, true);
            tw.Write(journalString);
            tw.Flush();
            return;
        }
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
            DeleteJournalFile(journalFilePath);
            string dynVersion = GetDynamoVersion(revitVersion);
            // Create journal string
            string journalString = BuildJournalStart();
            journalString += BuildProjectOpen(revitFilePath);
            journalString += BuildLaunchDynamo(workspacePath, revitVersion, dynVersion);
            journalString += BuildProjectClose();
            journalString += BuildJournalEnd();
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }
        /// <summary>
        /// Create a journal file for executing a Dynamo workspace on multiple Revit file in one continuous Revit session.
        /// 
        /// This journal file uses several keys specifically for the purpose of automating Dynamo.
        /// Dynamo is never run in the idle loop. The workspace is executed immediately, and control is returned to the DynamoRevit
        /// external application.
        /// </summary>
        /// <param name="revitFilePaths">The paths to the Revit files. These can be .rvt or .rfa files.</param>
        /// <param name="workspacePath">The path to the Dynamo workspace.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <returns>The path of the generated journal file.</returns>
        public static string ByModelPathsWorkspacePath(List<string> revitFilePaths, string workspacePath, string journalFilePath, int revitVersion)
        {
            DeleteJournalFile(journalFilePath);
            string dynVersion = GetDynamoVersion(revitVersion);
            // Create journal string
            string journalString = BuildJournalStart();
            foreach (string revitFilePath in revitFilePaths)
            {
                journalString += BuildProjectOpen(revitFilePath);
                journalString += BuildLaunchDynamo(workspacePath, revitVersion, dynVersion);
                journalString += BuildProjectClose();
            }
            journalString += BuildJournalEnd();
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }
        /// <summary>
        /// Create a journal file for executing multiple chained Dynamo workspaces on the same Revit file in one continuous Revit session.
        /// 
        /// This journal file uses several keys specifically for the purpose of automating Dynamo.
        /// Dynamo is never run in the idle loop. The workspace is executed immediately, and control is returned to the DynamoRevit
        /// external application.
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file.</param>
        /// <param name="workspacePaths">The paths to the Dynamo workspaces.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <returns>The path of the generated journal file.</returns>
        public static string ByModelPathWorkspacePaths(string revitFilePath, List<string> workspacePaths, string journalFilePath, int revitVersion)
        {
            DeleteJournalFile(journalFilePath);
            string dynVersion = GetDynamoVersion(revitVersion);
            // Create journal string
            string journalString = BuildJournalStart();
            journalString += BuildProjectOpen(revitFilePath);
            foreach (string workspacePath in workspacePaths)
            {
                journalString += BuildLaunchDynamo(workspacePath, revitVersion, dynVersion);
            }
            journalString += BuildProjectClose();
            journalString += BuildJournalEnd();
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }
        /// <summary>
        /// Create a journal file for executing multiple chained Dynamo workspaces on multiple Revit files in one continuous Revit session.
        /// 
        /// This journal file uses several keys specifically for the purpose of automating Dynamo.
        /// Dynamo is never run in the idle loop. The workspace is executed immediately, and control is returned to the DynamoRevit
        /// external application.
        /// </summary>
        /// <param name="revitFilePaths">The paths to the Revit files. These can be .rvt or .rfa files.</param>
        /// <param name="workspacePaths">The paths to the Dynamo workspaces.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <returns>The path of the generated journal file.</returns>
        public static string ByModelPathsWorkspacePaths(List<string> revitFilePaths, List<string> workspacePaths, string journalFilePath, int revitVersion)
        {
            DeleteJournalFile(journalFilePath);
            string dynVersion = GetDynamoVersion(revitVersion);
            // Create journal string
            string journalString = BuildJournalStart();
            foreach (string revitFilePath in revitFilePaths)
            {
                journalString += BuildProjectOpen(revitFilePath);
                foreach (string workspacePath in workspacePaths)
                {
                    journalString += BuildLaunchDynamo(workspacePath, revitVersion, dynVersion);
                }
                journalString += BuildProjectClose();
            }
            journalString += BuildJournalEnd();
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }
    }
}
