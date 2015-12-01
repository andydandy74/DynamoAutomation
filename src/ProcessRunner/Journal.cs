using System;
using System.IO;

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
        /// This journal file uses three keys specifically for the purpose of automating Dynamo.
        /// dynPath - Specifies the path to the Dynamo workspace to execute.
        /// dynShowUI - Specifies whether the Dynamo UI should be visible (set to false - Dynamo will run headless).
        /// dynAutomation - Specifies that Dynamo should run in 'automation' mode. This mode is similar to testing in that
        /// Dynamo is never run in the idle loop. The workspace is executed immediately, and control is returned to the DynamoRevit
        /// external application.
        /// </summary>
        /// <param name="revitFilePath">The path to the Revit file. This can be an .rvt or .rfa file.</param>
        /// <param name="workspacePath">The path to the Dynamo workspace.</param>
        /// <param name="journalFilePath">The path of the generated journal file.</param>
        /// <returns>The path of the generated journal file.</returns>
        public static string ByWorkspacePath(string revitFilePath, string workspacePath, string journalFilePath)
        {
            if (!File.Exists(workspacePath))
            {
                throw new FileNotFoundException();
            }

            if (File.Exists(journalFilePath))
            {
                File.Delete(journalFilePath);
            }

            using (var tw = new StreamWriter(journalFilePath, true))
            {
                var journal = String.Format(@"'" +
                                            "Dim Jrn \n" +
                                            "Set Jrn = CrsJournalScript \n" +
                                            "Jrn.Command \"StartupPage\" , \"Open this project , ID_FILE_MRU_FIRST\" \n" +
                                            "Jrn.Data \"MRUFileName\"  , \"{0}\" \n" +
                                            "Jrn.RibbonEvent \"Execute external command:CustomCtrl_%CustomCtrl_%Add-Ins%Visual Programming%Dynamo 0.9:Dynamo.Applications.DynamoRevit\" \n" +
                                            "Jrn.Data \"APIStringStringMapJournalData\", 3, \"dynPath\", \"{1}\", \"dynShowUI\", \"false\", \"dynAutomation\", \"true\" \n" +
                                            "Jrn.Command \"Internal\" , \"Flush undo and redo stacks , ID_FLUSH_UNDO\" \n" +
                                            "Jrn.Command \"Internal\" , \"Close the active project , ID_REVIT_FILE_CLOSE\" \n" +
                                            "Jrn.Command \"SystemMenu\" , \"Quit the application; prompts to save projects , ID_APP_EXIT\"",
                    revitFilePath.Replace(' ', '/'), workspacePath.Replace(' ', '/'));

                tw.Write(journal);
                tw.Flush();
            }

            return journalFilePath;
        }
    }
}
