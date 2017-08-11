# DynamoAutomation
DynamoAutomation is a package for the [Dynamo](https://github.com/DynamoDS/Dynamo) visual programming environment. It allows Dynamo users to batch process multiple Revit models by driving Revit (and the Dynamo addin) from the outside using the Dynamo sandbox, each time using the same Dynamo workflow, e.g. process an entire folder of models. This opens up a lot of possibilities for achieving a higher degree of automation in labour-intensive areas such as quality control and enforcement of company or project standards.

[DynamoAutomation Teaser Video](http://www.youtube.com/watch?v=vu4i-gEzzUo&autoplay=1)

## Requirements
Besides the DynamoAutomation package itself, you will need the following to successfully use DynamoAutomation:
- Any Dynamo 0.9.x or 1.x build
  - You must **not** have another version of Dynamo installed on the same machine.
- Revit 2015 or 2016
  - This needs to be an **English** language version.
  - DynamoAutomation should run on top of Revit 2015 but that hasn't been tested for a while now
- All folders that contain Revit models or Dynamo graphs (and their respective filenames) must **not** contain whitespaces.
- Revit models must **not** contain any warnings that would pop up when opening the model (e.g. missing linked files etc.).
- Revit models must **not** be saved in a Perspective view.

## Known bugs
No version of DynamoAutomation that is currently published on the package manager will run under Dynamo 1.x and Revit 2017/18. This bug has now been fixed for Revit 2017 and a new package version will soon be published after further tests in Revit 2018. If you are feeling adventurous, you can already download the package contents from this repository:

https://github.com/andydandy74/DynamoAutomation/tree/master/package

Note that the setup of both master and slave graphs has changed slightly, so be sure to visit the new Dynamo 1.x samples section:

https://github.com/andydandy74/DynamoAutomation/tree/master/samples/1.x

## Installation Guide
- Make sure to pick the correct version when downloading/installing
  - If you're running Dynamo 1.x use DynamoAutomation 1.0.0 or higher (works with Revit 2016, issues with Revit 2017 as described above)
  - If you're running Dynamo 0.9.x use DynamoAutomation 0.90.3 (works with Revit 2015 & 2016)
- DynamoAutomation has a dependency on the Clockwork package, but Clockwork is updated more frequently than Dynamo.
  - When installing DynamoAutomation, Dynamo will download and install a version of Clockwork that may not be current anymore.
  - Ater installing DynamoAutomation check the package manager to see if your installed version of Clockwork can be upgraded to a more recent version.

## How to Use DynamoAutomation
To get you started, this repository contains a number of [sample files](https://github.com/andydandy74/DynamoAutomation/tree/master/samples) for different automation scenarios. If you are looking for more in-depth information, please refer to the [DynamoAutomation wiki](https://github.com/andydandy74/DynamoAutomation/wiki).
 
## Updates & Version History
Since Dynamo's package manager currently does not (yet) have an update notification infrastructure in place, you may want to follow me on [twitter](https://twitter.com/a_dieckmann) for update notifications. Also, you can find the [version history](https://github.com/andydandy74/DynamoAutomation/wiki/Version-History) on the wiki.
