using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.Attributes;
using  Autodesk.Revit.DB.Events;

namespace DynamoAutomation
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    public class Swallower : IExternalApplication
    {
    	private static bool journaling;
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
            	var app = application.ControlledApplication;
            	string recJournal = app.RecordingJournalFilename;
            	if (recJournal.Contains(@"ProgramData\Autodesk\Revit\") )
            	{
            		//we're in journaling mode
            		journaling = true;
            		dialogHandler = new EventHandler<DialogBoxShowingEventArgs>(DismissAllDialogs);
            		warningsHandler = new EventHandler<FailuresProcessingEventArgs>(DismissAllWarnings);
            		application.DialogBoxShowing += dialogHandler;
            		app.FailuresProcessing += warningsHandler;
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
        	if (journaling)
        	{
        		application.DialogBoxShowing -= dialogHandler;
        		application.ControlledApplication.FailuresProcessing -= warningsHandler;
        	}
        	return Result.Succeeded;
        }

        /// <summary>
        /// Will dismiss all undesired dialogs
        /// </summary>
        private void DismissAllDialogs(object o, DialogBoxShowingEventArgs e)
        {
        	e.OverrideResult(1);
//            TaskDialogShowingEventArgs t = e as TaskDialogShowingEventArgs;
//            if (t != null)
//            {
//                // Call OverrideResult to cause the dialog to be dismissed with the specified return value ("No")
//                // (int) is used to convert the enum TaskDialogResult.No to its integer value which is the data type required by OverrideResult
//                e.OverrideResult((int)TaskDialogResult.No);
//            }
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
    
//    /// <summary>
//    /// Check journal data
//    /// </summary>
//
//    public class ExtCmdCheckJournalData : IExternalCommand
//    {
//        public Result Execute(ExternalCommandData cmdData, ref string msg, ElementSet elemSet)
//        {
//            // DIMITAR: This needs to be refined to actually access the key we want
//            if (cmdData.JournalData == null)
//            {
//                return Result.Failed;
//            }
//            else
//            {
//                return Result.Succeeded;
//            }
//        }
//    }
}