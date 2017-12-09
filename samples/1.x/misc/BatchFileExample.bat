@echo off 
:: Make sure to start all calls with the /W option in order to execute them successively
echo This is an example for using DynamoAutomation with Windows Task Scheduler
:: Change to Dynamo's working directory
cd C:\Program Files\Dynamo\Dynamo Core\1.3
@echo on
:: Run your master graph with the DynamoCLI
start /W DynamoCLI.exe -o "C:\temp\DynamoAutomation\master_graphs\Scenario_A1_SchedulableWithConfig.dyn"
:: Due to a bug in the DynamoCLI your master graph needs to create a second batch file that will start the Revit sessions
:: Call that batch file after your master graph is done
cd C:\temp\DynamoAutomation\output
start /W example.bat
@echo off 
echo All done...
:: Use the pause command to keep the window from closing automatically
pause