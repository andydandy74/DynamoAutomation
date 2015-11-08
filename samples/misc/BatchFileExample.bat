@echo off 
echo This is an example for calling multiple Dynamo batch-processes...
:: Change to Dynamo's working directory
cd C:\Program Files\Dynamo 0.8
@echo on
:: List all calls to the DynamoCLI below...
:: Make sure to call them with the /W option in order to execute them successively
start /W DynamoCLI.exe -o "C:\temp\DynamoAutomation\master_graphs\Scenario_A2.dyn"
start /W DynamoCLI.exe -o "C:\temp\DynamoAutomation\master_graphs\Scenario_A1_withPresets.dyn" /s "all"
@echo off 
echo All done...
:: Use the pause command to keep the window from closing automatically
pause