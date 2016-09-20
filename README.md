#DynamoAutomation
DynamoAutomation is a package for the [Dynamo](https://github.com/DynamoDS/Dynamo) visual programming environment. It allows Dynamo users to batch process multiple Revit models by driving Revit (and the Dynamo addin) from the outside using the Dynamo sandbox, each time using the same Dynamo workflow, e.g. process an entire folder of models. This opens up a lot of possibilities for achieving a higher degree of automation in labour-intensive areas such as quality control and enforcement of company or project standards.

[DynamoAutomation Teaser Video](http://www.youtube.com/watch?v=vu4i-gEzzUo&autoplay=1)

##Requirements
Besides the DynamoAutomation package itself, you will need the following to successfully use DynamoAutomation:
- Any Dynamo 0.9.x or 1.x build
  - You must **not** have another version of Dynamo installed on the same machine
- Revit 2015 or higher
  - This needs to be an **English** language version.
- All folders that contain Revit models or Dynamo graphs (and their respective filenames) must **not** contain whitespaces.
- Revit models must **not** contain any warnings that would pop up when opening the model (e.g. missing linked files etc.)

##How to Use DynamoAutomation
To get you started, this repository contains a number of [sample files](https://github.com/andydandy74/DynamoAutomation/tree/master/samples) for different automation scenarios. If you are looking for more in-depth information, please refer to the [DynamoAutomation wiki](https://github.com/andydandy74/DynamoAutomation/wiki).
 
##Updates & Version History
Since Dynamo's package manager currently does not (yet) have an update notification infrastructure in place, you may want to follow me on [twitter](https://twitter.com/a_dieckmann) for update notifications. Also, you can find the [version history](https://github.com/andydandy74/DynamoAutomation/wiki/Version-History) on the wiki.
