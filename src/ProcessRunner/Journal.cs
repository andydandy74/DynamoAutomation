using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        private static string GetDynamoVersion(dynamic revitVersion)
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
        /// Returns the Revit version (entered as string, double or integer) as an integer
        /// </summary>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <returns>The Revit version as an integer</returns>
        private static int RevitVersionAsInt(dynamic revitVersion)
        {
            int revitVersionAsInt;
            Type t = revitVersion.GetType();
            if (t.Equals(typeof(int)))
            {
                revitVersionAsInt = revitVersion;
            }
            else if (t.Equals(typeof(double)))
            {
                revitVersionAsInt = Convert.ToInt32(revitVersion);
            }
            else if (t.Equals(typeof(string)))
            {
                revitVersionAsInt = int.Parse(revitVersion);
            }
            else
            {
                throw new ArgumentException("Revit Version could not be derived from input", "revitVersion");
            }
            return revitVersionAsInt;
        }

        /// <summary>
        /// Verifies that the given file path is a valid path for usage in journal files
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <param name="paramName">The name of the parameter for the path</param>
        /// <returns>A boolean to indicate success/failure</returns>
        private static bool CheckFilePath(string filePath, string paramName)
        {
            // This regular expression finds all but the following characters:
            // 0-1 a-z A-Z (no accented characters like Umlaute)
            // also _ - + { } ( ) [ ] . : \
            string regexpat = "[^0-9\\u0041-\\u005A\\u0061-\\u007A-_+:{}()\\[\\]\\.\\\\]";
            Regex regexp = new Regex(regexpat);
            MatchCollection matches;
            matches = regexp.Matches(filePath);
            if (matches.Count > 0)
            {
                List<string> str_matches = new List<string>();
                for (int i = 0; i < matches.Count; i++)
                {
                    if (!str_matches.Contains(matches[i].Value))
                    {
                        str_matches.Add(matches[i].Value);
                    }
                }
                string exmsg = "The file path is not valid because it contains characters not compatible with Revit journal files: ";
                for (int i = 0; i < str_matches.Count; i++)
                {
                    exmsg += "[" + str_matches[i] + "], ";
                }
                exmsg = exmsg.Replace("[ ]", "[WHITESPACE]");
                exmsg = exmsg.Substring(0, exmsg.Length - 2);
                throw new ArgumentException(exmsg, paramName);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Builds the first part of the journal string
        /// </summary>
        /// <param name="debugMode">Should the journal file be run in debug mode?</param>
        /// <returns>The first part of the journal string.</returns>
        private static string BuildJournalStart(bool debugMode = false)
        {
            string journalStart = "'" +
                "Dim Jrn \n" +
                "Set Jrn = CrsJournalScript \n";
            // These two directives can make things easier if needed
            if (debugMode)
            {
                journalStart += "Jrn.Directive \"DebugMode\", \"PerformAutomaticActionInErrorDialog\", 1 \n";
                journalStart += "Jrn.Directive \"DebugMode\", \"PermissiveJournal\", 1 \n";
            }
            return journalStart;
        }
        
        /// <summary>
        /// Builds the part of the journal string responsible for opening a project
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file.</param>
        /// <returns>The part of the journal string responsible for opening a project.</returns>
        private static string BuildProjectOpen(string revitFilePath)
        {
            CheckFilePath(revitFilePath, "revitFilePath");
            // Exception if the rvt/rfa file isn't found
            if (!File.Exists(revitFilePath))
            {
                throw new FileNotFoundException();
            }
            string projectOpen = String.Format("Jrn.Command \"StartupPage\" , \"Open this project , ID_FILE_MRU_FIRST\" \n" +
                                            "Jrn.Data \"MRUFileName\" , \"{0}\" \n", revitFilePath);
            return projectOpen;
        }
        
        /// <summary>
        /// Builds the part of the journal string responsible for launching DynamoRevit
        /// </summary>
        /// <param name="workspacePath">The path to the Dynamo workspace.</param>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <param name="dynVersion">The version number of Dynamo (e.g. 1.3).</param>
        /// <returns>The part of the journal string responsible for launching DynamoRevit.</returns>
        private static string BuildDynamoLaunch(string workspacePath, dynamic revitVersion, string dynVersion)
        {
            CheckFilePath(workspacePath, "workspacePath");
            // Exception if the dyn file isn't found
            if (!File.Exists(workspacePath))
            {
                throw new FileNotFoundException();
            }
            string launchDynamo = "";
            // Launch command and journal keys differ between pre-2017 and post-2016 Revit
            if (revitVersion > 2016)
            {
                // Doesn't work with Dynamo 1.0
                launchDynamo += String.Format("Jrn.Command \"Ribbon\" , \"Launch Dynamo, ID_VISUAL_PROGRAMMING_DYNAMO\" \n" +
                    "Jrn.Data \"APIStringStringMapJournalData\", 5, \"dynPath\", \"{0}\", \"dynShowUI\", \"false\", \"dynAutomation\", \"false\", \"dynPathExecute\", \"true\", \"dynModelShutDown\", \"true\" \n", workspacePath);
            }
            else
            {
                launchDynamo += String.Format("Jrn.RibbonEvent \"Execute external command:CustomCtrl_%CustomCtrl_%Add-Ins%Visual Programming%Dynamo {0}:Dynamo.Applications.DynamoRevit\" \n" +
                    "Jrn.Data \"APIStringStringMapJournalData\", 3, \"dynPath\", \"{1}\", \"dynShowUI\", \"false\", \"dynAutomation\", \"true\" \n", dynVersion, workspacePath);
                // This command must only be called in pre-2017 Revit versions
                launchDynamo += "Jrn.Command \"Internal\" , \"Flush undo and redo stacks , ID_FLUSH_UNDO\" \n";
            }
            return launchDynamo;
        }

        /// <summary>
        /// Builds the part of the journal string responsible for purging the model
        /// </summary>
        /// <returns>The part of the journal string responsible for purging the model.</returns>
        private static string BuildProjectPurge()
        {
            string projectPurge = "";
            // Execute Purge three times in a row to make sure that *everything* has been purged
            for (int i = 0; i < 3; i += 1)
            {
                projectPurge += "Jrn.Command \"Ribbon\" , \"Purge(delete) unused families and types, ID_PURGE_UNUSED\" \n";
                projectPurge += "Jrn.PushButton \"Modal , Purge unused, Dialog_Revit_PurgeUnusedTree\" , \"OK, IDOK\" \n";
                projectPurge += "Jrn.Data \"Transaction Successful\" , \"Purge unused\" \n";
            }
            return projectPurge;
        }

        /// <summary>
        /// Builds the part of the journal string responsible for saving a project
        /// </summary>
        /// <returns>The part of the journal string responsible for saving a project.</returns>
        private static string BuildProjectSave()
        {
            string projectSave = "Jrn.Command \"Ribbon\" , \"Save the active project , ID_REVIT_FILE_SAVE\" \n";
            return projectSave;
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
        /// Builds the last part of the journal string
        /// </summary>
        /// <returns>The last part of the journal string.</returns>
        private static string BuildJournalEnd()
        {
            string journalEnd = "Jrn.Command \"SystemMenu\" , \"Quit the application; prompts to save projects , ID_APP_EXIT\"";
            return journalEnd;
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
        /// Create a journal file for purging and subsequently saving a Revit file.
        /// 
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file. The path may not contain whitespace or accented characters.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <returns>The path of the generated journal file.</returns>
        public static string PurgeModel(string revitFilePath, string journalFilePath)
        {
            DeleteJournalFile(journalFilePath);
            // Create journal string
            // Journal needs to be in debug mode. 
            // Otherwise the journal playback may stop if there is nothing to purge.
            string journalString = BuildJournalStart(true);
            journalString += BuildProjectOpen(revitFilePath);
            journalString += BuildProjectPurge();
            journalString += BuildProjectSave();
            journalString += BuildProjectClose();
            journalString += BuildJournalEnd();
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }

        /// <summary>
        /// Create a journal file for purging and subsequently saving multiple Revit files in a single Revit session.
        /// 
        /// </summary>
        /// <param name="revitFilePaths">The paths to the Revit files. These can be .rvt or .rfa files. The paths may not contain whitespace or accented characters.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <returns>The path of the generated journal file.</returns>
        public static string PurgeModels(List<string> revitFilePaths, string journalFilePath)
        {
            DeleteJournalFile(journalFilePath);
            // Create journal string
            string journalString = BuildJournalStart(true);
            // Open, purge, save and close all models
            foreach (string revitFilePath in revitFilePaths)
            {
                journalString += BuildProjectOpen(revitFilePath);
                journalString += BuildProjectPurge();
                journalString += BuildProjectSave();
                journalString += BuildProjectClose();
            }
            journalString += BuildJournalEnd();
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }

        /// <summary>
        /// Create a journal file for executing a Dynamo workspace on a Revit file.
        /// 
        /// This journal file uses several keys specifically for the purpose of automating Dynamo.
        /// Dynamo is never run in the idle loop. The workspace is executed immediately, and control is returned to the DynamoRevit
        /// external application.
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file. The path may not contain whitespace or accented characters.</param>
        /// <param name="workspacePath">The path to the Dynamo workspace. The path may not contain whitespace or accented characters.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <param name="revitVersion">The version number of Revit (e.g. 2017).</param>
        /// <param name="debugMode">Should the journal file be run in debug mode? Set this to true if you expect models to have warnings (e.g. missing links etc.).</param>
        /// <returns>The path of the generated journal file.</returns>
        public static string ByWorkspacePath(string revitFilePath, string workspacePath, string journalFilePath, dynamic revitVersion, bool debugMode = false)
        {
            DeleteJournalFile(journalFilePath);
            int revitVersionInt = RevitVersionAsInt(revitVersion);
            string dynVersion = GetDynamoVersion(revitVersionInt);
            // Create journal string
            string journalString = BuildJournalStart(debugMode);
            journalString += BuildProjectOpen(revitFilePath);
            journalString += BuildDynamoLaunch(workspacePath, revitVersionInt, dynVersion);
            // In newer Revit versions the slave graph will only run if there are no journal commands after launching Dynamo.
            // The slave graph will then need to terminte itself.
            if (revitVersionInt < 2017)
            {
                journalString += BuildProjectClose();
                journalString += BuildJournalEnd();
            }
            // Create journal file
            WriteJournalFile(journalFilePath, journalString);
            return journalFilePath;
        }
    }
}
