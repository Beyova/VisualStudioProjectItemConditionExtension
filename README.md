# Project Item Condition Extension [![Build status](https://ci.appveyor.com/api/projects/status/vm044ecjq1l7ecn3?svg=true)](https://ci.appveyor.com/project/rynnwang/visualstudioprojectitemconditionextension)

## Background
Applying MS build condition based on specific file(s) is not a common case, but when you have to face this kind of case, you would always feel upset to manually change XML project file directly.

## Typical case of apply MS build condition:
- File configurations, like based on different environments
- Resource files, images, localizations, etc. for different environment or purposes.
- Source codes for different environment or purposes.

## How to install extension
In Visual Studio Extension Marketplace, search and install [*Project Item Condition Extension*](https://marketplace.visualstudio.com/items?itemName=RynnWang.ProjectItemConditionExtension).

## How to use extension
![Image of preview](images/preview.png)
### Applying condition on file(s)
When your solution/project are opened in Visual Studio
-> Select any source code file
-> Right click and select *Apply Item Condition* menu item
-> In popup dialog, select the build condition you want to apply
-> Click *Apply* button
-> Done. (Visual Studio would remind you to reload solution/projects. Please do it and condition take effected.)


### Initialize envrionment based build configuration
When your solution/project are opened in Visual Studio
-> Select any source code file
-> Right click and select *Setup Configuration (DEV/QA/STAGING/PROD)* menu item
-> Done. (It would update and save all documents)

## Others.
- This extension is built based on Framework 4.6
- This extension would be installed and work on 
    - Visual Studio 2017
- Source Code: https://github.com/Beyova/VisualStudioProjectItemConditionExtension