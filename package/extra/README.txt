##Instructions
- You'll need a daily build from September 16th or later for this to work
- The files in the **dyf** folder need to be copied to your definitions folder (C:\Users\YOUR_USERNAME\AppData\Roaming\Dynamo\0.8\definitions).
- Copy the subfolders of the **extra** folder to C:\temp\DynamoAutomation and run any of the master graphs.
- After you've opened the first master graph you'll need to import the zero touch library located in the **bin** folder - you may have to replace the two unresolved nodes after that (Find them in the DynamoAutomation category).
- Reports and journal files will be written to C:\temp\DynamoAutomation\output

##Known issues
- Multiple CSV file read/write operations will result in multiple whitespaces being added between delimiters and data. I have submitted a fix for this (https://github.com/DynamoDS/Dynamo/pull/5324) so this may already be fixed in the latest dailies.
- Subcategory structure for DynamoAutomation still needs a bit of cleanup.
- Journal file creation for directory names that contain whitespaces.
- Currently no solution for scenario C (export, modify & reload families) due to a transaction error.