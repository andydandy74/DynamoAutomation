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
		#region private properties
		
		private static bool journalModeIsPermissive;
		private static bool journalChecked;
		
		private static UIControlledApplication uiCtrlApp;
		private static EventHandler<DocumentOpeningEventArgs> openingHandler;
		private static EventHandler<DocumentClosingEventArgs> closingHandler;
		private static EventHandler<DialogBoxShowingEventArgs> dialogHandler;
		private static EventHandler<FailuresProcessingEventArgs> warningsHandler;
		
		private static int popupsNum;
		private static int warningsNum;
		private static string currDocName;
		private static string reportPath;
		
		#endregion
		
		
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
				journalChecked = false;
				journalModeIsPermissive = false;
				uiCtrlApp = application;
				// This handler will be disconnected at the end of the opening trigger
				openingHandler = new EventHandler<DocumentOpeningEventArgs>(ActivateSwallowers);
				application.ControlledApplication.DocumentOpening += openingHandler;
				string jrnPath = application.ControlledApplication.RecordingJournalFilename;
				jrnPath.Replace(".txt.", "");
				reportPath = jrnPath + "_ErrorReport.txt";
				return Result.Succeeded;
			}
			
			catch (Exception ex)
			{
				//don't use a dialog to display these, we might block those! :)
				System.Windows.Forms.MessageBox.Show(ex.ToString(), "DynamoAutomation Error");
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
				uiCtrlApp.ControlledApplication.DocumentOpening -= openingHandler;
				if (journalModeIsPermissive)
				{
					application.DialogBoxShowing -= dialogHandler;
					application.ControlledApplication.FailuresProcessing -= warningsHandler;
					application.ControlledApplication.DocumentClosing -= closingHandler;
				}
				return Result.Succeeded;
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.ToString(), "DynamoAutomation Error");
				return Result.Failed;
			}
		}

		/// <summary>
		/// Assigns event handlers for DialogBoxShowing and FailuresProcessing if we're in debug mode
		/// journal playback
		/// </summary>
		private void ActivateSwallowers(object o, DocumentOpeningEventArgs e)
		{
			currDocName = Path.GetFileNameWithoutExtension(e.PathName);
			warningsNum = 0;
			popupsNum = 0;
			
			// And we'll only want to add them if the journal is actually running in permissive mode
			if (!journalChecked && CheckJournalingMode() )
			{
				dialogHandler = new EventHandler<DialogBoxShowingEventArgs>(DismissAllDialogs);
				warningsHandler = new EventHandler<FailuresProcessingEventArgs>(DismissAllWarnings);
				closingHandler = new EventHandler<DocumentClosingEventArgs >(PopulateReport);
				uiCtrlApp.DialogBoxShowing += dialogHandler;
				uiCtrlApp.ControlledApplication.FailuresProcessing += warningsHandler;
				uiCtrlApp.ControlledApplication.DocumentClosing += closingHandler;
				journalChecked = true;
			}
		}

		/// <summary>
		/// Checks if Revit is run in journal playback mode and permissive journaling is activated
		/// </summary>
		private bool CheckJournalingMode()
		{
			string recJournal = uiCtrlApp.ControlledApplication.RecordingJournalFilename;
			// https://msdn.microsoft.com/en-us/library/ms182334.aspx
			// We'll want to use StreamReader for speed, but need to be careful regarding
			// shared access, hence opening via FileStream */
			Stream stream = null;
			try
			{
				stream = new FileStream(recJournal, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using (var reader = new StreamReader(stream, Encoding.Unicode) )
				{
					stream = null;
					string line = null;
					while (!reader.EndOfStream)
					{
						line = reader.ReadLine();
						if (line.Contains("Jrn.Directive \"DebugMode\"") )
						{
							// Set this flag so we know we should remove event handlers OnShutdown
							journalModeIsPermissive = true;
							return true;
						}
					}
					return false;
				}
			}
			finally
			{
				if(stream != null)
					stream.Dispose();
			}
		}
		
		private void PopulateReport(object o, DocumentClosingEventArgs e)
		{
			PopulateReport();
		}
		
		private void PopulateReport()
		{
			string line;
			if (warningsNum != 0 || popupsNum != 0)
			{
				line = String.Format("Document '{0}' contained {1} warnings and {2} popups{3}",
				                     currDocName, warningsNum, popupsNum, Environment.NewLine);
			}
			else
			{
				line = String.Format("Document '{0}' contained NO warnings & popups{1}",
				                     currDocName, Environment.NewLine);
			}
			try
			{
				File.AppendAllText(reportPath, line);
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(ex.ToString(), "DynamoAutomation Error");
			}
		}

		/// <summary>
		/// Will dismiss all dialogs
		/// </summary>
		private void DismissAllDialogs(object o, DialogBoxShowingEventArgs e)
		{
			e.OverrideResult(1);
			popupsNum += 1;
		}

		/// <summary>
		/// Will dismiss all warnings
		/// </summary>
		private void DismissAllWarnings(object o, FailuresProcessingEventArgs e)
		{
			FailuresAccessor fa = e.GetFailuresAccessor();
			IList<FailureMessageAccessor> failList = fa.GetFailureMessages();
			// Inside event handler, get all warnings
//			warningsNum += failList.Count;
			foreach (FailureMessageAccessor failure in failList)
			{
				fa.DeleteWarning(failure);
				warningsNum += 1;
			}
		}
	}
}
