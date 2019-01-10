[![Current 1.x version](https://img.shields.io/badge/current%201.x%20version-1.31.1-brightgreen.svg)](http://dynamopackages.com/) ![Current 2.x version](https://img.shields.io/badge/current%202.x%20version-none-brightgreen.svg) ![Node count](https://img.shields.io/badge/node%20count-11-brightgreen.svg) [![Dependency](https://img.shields.io/badge/dependency-Clockwork-brightgreen.svg)](https://github.com/andydandy74/ClockworkForDynamo) [![Contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/andydandy74/DynamoAutomation/blob/master/.github/CONTRIBUTING.md) [![Follow me on Twitter for updates](https://img.shields.io/twitter/follow/a_dieckmann.svg?label=Follow&style=social)](https://twitter.com/a_dieckmann)

![Image](https://raw.githubusercontent.com/andydandy74/DynamoAutomation/master/icon/raw/DynamoAutomationLogo.png)

**DynamoAutomation** is a package for the [Dynamo](http://www.dynamobim.com) visual programming environment. It allows Dynamo users to batch process or batch purge multiple Revit models by driving Revit (and the Dynamo addin) from the outside using the Dynamo sandbox, each time using the same Dynamo workflow, e.g. process an entire folder of models. This opens up a lot of possibilities for achieving a higher degree of automation in labour-intensive areas such as quality control and enforcement of company or project standards.

You can find the [version history](https://github.com/andydandy74/DynamoAutomation/wiki/Version-History) on the wiki.

[RTCEUR 2015 DynamoAutomation Teaser Video](http://www.youtube.com/watch?v=vu4i-gEzzUo&autoplay=1)

**DynamoAutomation has not yet been tested with Dynamo 2.x - any feedback welcome.**

## Requirements
Besides the DynamoAutomation package itself, you will need the following to successfully use DynamoAutomation:
- Any Dynamo 0.9.x or 1.x build
  - You must **not** have more than one version of Dynamo installed on the same machine.
- Revit 2015 or later
  - DynamoAutomation should run on top of Revit 2015 but that hasn't been tested for a while now
- All folders that contain Revit models or Dynamo graphs (and their respective filenames) must **not** contain whitespaces or special characters (e.g. accented characters like an umlaut).
- Workshared models may **not** be set to Specify Worksets on open.

If you have the Screencast addin installed, it may be advisable to disable when processing larger numbers of models.

## Installation Guide
- Make sure to pick the correct version when downloading/installing
  - If you're running Dynamo 1.x use the latest version of DynamoAutomation
  - If you're running Dynamo 0.9.x use DynamoAutomation 0.90.3 (works with Revit 2015 & 2016)
- DynamoAutomation has a dependency on the Clockwork package, but Clockwork is updated more frequently than DynamoAutomation.
  - When installing DynamoAutomation, Dynamo will download and install a version of Clockwork that may not be current anymore.
  - Ater installing DynamoAutomation check the package manager to see if your installed version of Clockwork can be upgraded to a more recent version.
  - Alternatively, download DynamoAutomation from http://www.dynamopackages.com (without Clockwork) and copy manually to your Dynamo packages directory.

## How to Use DynamoAutomation
To get you started, this repository contains a number of [sample files](https://github.com/andydandy74/DynamoAutomation/tree/master/samples) for different automation scenarios. If you are looking for more in-depth information, please refer to the [DynamoAutomation wiki](https://github.com/andydandy74/DynamoAutomation/wiki).

![Bad Monkeys logo](https://www.badmonkeys.net/wp-content/uploads/2016/12/BadMonkey_finalLogo-01.png)
