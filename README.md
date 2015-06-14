# Dynamo Automation
Examples for batch-processing of Revit models using Dynamo

##Current Dynamo limitations
- Dynamo can only process one model at a time
    - Great for project-specific work, not so great for global recurring tasks
    - Running the same graph on multiple models = a lot of tedious manual labour
- Dynamo cannot switch between files (e.g. project vs. family)
    - Show stopper for work flows that require family editing 

## Typical abstract scenarios
- A1: Run the same or multiple graph(s) on all models in a folder
- A2: Run the same or multiple graph(s) on all models in a list of folders
- A3: Run the same or multiple graph(s) on a list of models at various locations
- A4: Run multiple graphs on multiple files according to a matrix
- B: Run one graph to find all links in a project, run a second graph on all links
- C: Run one graph to export certain families from a project, run a second graph on all families, run a third graph to load all families back into the project

## Typical concrete scenarios aka actual real life use cases
- Original use case
    - BIM.Basics model checker (https://prezi.com/kaagf4w6iq-d/the-truth-is-in-the-model/)
- Some typical QC examples
    - One cleanup task (change all elements that do not comply to a given rule)
    - One markup task (mark all elements that do not comply to a given rule)
    - One report task (report all elements that do not comply to a given rule)
        - Project-based reports
        - Aggregated report
- Examples of family editing
    - Change font in all annotation families and reload into project (http://dynamobim.com/forums/topic/font-type-in-revit-using-dynamo/) 
    - Create multiple mass families (http://dynamobim.com/forums/topic/can-dynamo-import-geometry-and-make-new-family/) 
    - Evaluate multiple families (http://dynamobim.com/forums/topic/can-dynamo-import-geometry-and-make-new-family/) 
