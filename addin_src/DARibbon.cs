using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace DynamoAutomation
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Swallower : IExternalApplication
    {
    	private static bool journalModeChecked;
        private static bool journalModeIsPermissive;
        private static UIControlledApplication uiCtrlApp;
        private static EventHandler<DocumentOpeningEventArgs> openingHandler;
        private static EventHandler<DialogBoxShowingEventArgs> dialogHandler;
        private static EventHandler<FailuresProcessingEventArgs> warningsHandler;
        /// <summary>
        /// Implements the external application which should be called when 
        /// Revit starts before a file or default template is actually loaded.
        /// </summary>
        /// <param name="application">The controlled application.</param>
        /// <returns>Return the status of the external application.</returns>
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Add this event handler only once, not every time a document is about to be opened
                if (!journalModeChecked)
                {
                    openingHandler = new EventHandler<DocumentOpeningEventArgs>(ActivateSwallowers);
                    application.ControlledApplication.DocumentOpening += openingHandler;
                }           
            	return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("DynamoAutomation", ex.ToString() );
                return Result.Failed;
            }
        }

        /// <summary>
        /// Implements the external application which should be called when Revit is about to exit.
        /// </summary>
        /// <param name="application">The controlled application.</param>
        /// <returns>Return the status of the external application.</returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                // Only try to remove the event handlers if they were actually assigned
                if (journalModeChecked)
                {
                    application.ControlledApplication.DocumentOpening -= openingHandler;
                }
                if (journalModeIsPermissive)
                {
                    application.DialogBoxShowing -= dialogHandler;
                    application.ControlledApplication.FailuresProcessing -= warningsHandler;
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("DynamoAutomation", ex.ToString());
                return Result.Failed;
            }
        }

        /// <summary>
        /// Assigns event handlers for DialogBoxShowing and FailuresProcessing if we're in debug mode journal playback
        /// </summary>
        private void ActivateSwallowers(object o, DocumentOpeningEventArgs e)
        {
            // Try to add these event handlers only once, not every time a document is about to be opened
            if (!journalModeChecked)
            {
                // And we'll only want to add them if the journal is actually running in permissive mode
                if (CheckJournalingMode() && journalModeIsPermissive)
                {
                    dialogHandler = new EventHandler<DialogBoxShowingEventArgs>(DismissAllDialogs);
                    warningsHandler = new EventHandler<FailuresProcessingEventArgs>(DismissAllWarnings);
                    uiCtrlApp.DialogBoxShowing += dialogHandler;
                    uiCtrlApp.ControlledApplication.FailuresProcessing += warningsHandler;
                }
            }        
        }

        /// <summary>
        /// Checks if Revit is run in journal playback mode and permissive journaling is activated
        /// </summary>
        private bool CheckJournalingMode()
        {
            string recJournal = uiCtrlApp.ControlledApplication.RecordingJournalFilename;
            // We'll want to use StreamReader for speed, but need to be careful regarding shared access, hence opening via FileStream
            using (var file = new FileStream(recJournal, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) using (var reader = new StreamReader(file, Encoding.Unicode))
            {
                // Always set journalModeChecked so this function only gets called once per session
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line.Contains("Jrn.Directive \"DebugMode\", \"PermissiveJournal\", 1"))
                    {
                        journalModeChecked = true;
                        // Set this flag so we know we should remove event handlers OnShutdown
                        journalModeIsPermissive = true;
                        return true;
                    }
                }
                journalModeChecked = true;
                return false;
            }
        }

        /// <summary>
        /// Will dismiss all dialogs
        /// </summary>
        private void DismissAllDialogs(object o, DialogBoxShowingEventArgs e)
        {
        	e.OverrideResult(1);
        }

        /// <summary>
        /// Will dismiss all warnings
        /// </summary>
        private void DismissAllWarnings(object o, FailuresProcessingEventArgs e)
        {
            FailuresAccessor fa = e.GetFailuresAccessor();
            IList<FailureMessageAccessor> failList = fa.GetFailureMessages();
            // Inside event handler, get all warnings
            foreach (FailureMessageAccessor failure in failList)
            {
                fa.DeleteWarning(failure);
            }
        }  
    }
}