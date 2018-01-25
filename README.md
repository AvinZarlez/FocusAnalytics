# FocusAnalytics
Heat map analysis reporting for Mixed Reality applications.

## Introduction
This hack is aimed to allow for data collection of users Gaze/Focus in mixed reality scenarios. When a user is viewing an IFocusable object with a AnalyticsFocusTarget behavior, this captures the amount of time the user spent viewing the object and then logs the data to an Azure EasyTable.   

The data can then be broken down based on timeframe and/or device. 

## Getting Started
1. Requirements
    There are a few requirements for this to work with your project.
    1. [Mixed Reality Toolkit for Unity](https://github.com/Microsoft/MixedRealityToolkit-Unity)

        We used version v1.2017.2.0

    1. "InputManager" from the toolkit must be included in your scene. 
    1. You may need to disable the two Newtonsoft.Json.dll files from within the HoloToolkit folder.

        This project includes its own Newtonsoft.Json.dll files, so nothing in the toolkit *should* break.
        There are two versions of this dll, one for one of the editor/standlone and one for WSA.
        In the versions in the Mixed Reality Toolkit, uncheck everything. Leave versions in the FocusAnalytics folder unchanged.
    1. Azure setup
        - In the Azure portal create a Mobile App. Once the deployment completes, go the Overview page and copy the Mobile App URI Example: http://yourapphere.azurewebsites.net
        - Under Mobile Menu create an Easy Table. Click on the prompt to configure Easy Tables/Easy APIs
            - First click on "Connect to a database" and create a database connection.
            - **CRITICAL:** When creating the database connection, be sure to name the connection string *MS_TableConnectionString*
            - Refresh and then click on "Create TodoItem"
        - Under the Easy Table menu  click on the plus sign to add a new table and call it "**ReportableFocusEvent**".  

1. Import FocusAnalytics
1. Track Objects

    For each object you would like to track, add AnalyticsFocusTarget behavior component to the game object
1. Add the AnalyticsFocusReporter prefab into you scene

    The prefab has the following parameters:

    - You can set the "push interval" upload parameter based on your requirements. Set in number of seconds, minimum 15 seconds, default max 10 minutes (600 seconds)
	- Adjust Azure subscription information by replacing the Mobile App URI. Example: http://yourapphere.azurewebsites.net

## Code Breakdown

- AnalyticsFocusReporter.cs 

    A script that powers AnalyticsFocusReporter prefab.

    Communicates with Azure table services. Handles local SQLite DB.

- AnalyticsFocusReporter.prefab

    Required to be included in your scene.

    Allows editing of parameters in the Unity editor.

- AnalyticsFocusTarget.cs

    Upon focus enter, stamps start time. When focus exits, performs math for duration, then sends ReportableFocusEvent to AnalyticFocusReporter.
    
    If Visualize is set to true, will change the material color to Red on focus enter, then back to original upon focus exit. 

- HardwareIdentification.cs

    Get UUID of user/device.

- ReportableFocusEvent.cs

    Provides data structure to be stored on Azure. This script has the same name as the EasyTable you created on Azure earlier.

    - ID - Internal ID for the Azure Table
    - PackageSpecficToken - A unique identification number of the headset/system via ASHWID (App Specific Hardware ID) 
    - Label  - Name of object within Mixed Reality space
    - Start - Start time of gaze by user
    - Duration - Length of time of current gaze session
    - Position - X,Y,Z location of the object in Mixed Reality space

- AnalyticsExampleScene.unity
    A sample scene for this project.

- AnalyticsFocusObject.prefab

    A prefab object to be rendered by the AnalyticsFocusRenderer

- AnalyticsFocusMaterial.mat

    A Material for the AnalyticsFocusObject prefab.

- AnalyticsFocusRenderer.prefab

    A prefab to be included in the scene to use the AnalyticsFocusRenderer.

    Click the "Render Table" button to render AnalyticsFocusObjects.

- AnalyticsFocusRenderer.cs
    
    Script to make AnalyticsFocusRenderer work.

## Contribute
We'd love your help improving this hack!

If you want to learn more about integrating Azure with Unity, we based our code (and dll) needs from [this repo](https://github.com/BrianPeek/AzureSamples-Unity) maintained by Brian Peek. Thanks Brian for helping us find the required dlls!

[Here is a link](https://docs.microsoft.com/en-us/sandbox/gamedev/unity/azure-mobile-apps-unity) to the official documentation.

We also recommend getting the latest version of the [Mixed Reality Toolkit Unity](https://github.com/Microsoft/MixedRealityToolkit-Unity).

## Credits
Created as part of a Mixed Reality focused hackathon.

Team:
- Tobiah Zarlez
- Doug Holland
- Mihaela Curmei
- Tommy Patterson

Huge thanks to:
- Adam Tuliper
- Brian Peek

Ongoing development beyond hackathon:
- Jared Bienz


## Attribution
The following 3D models were kindly offered as [CC0](https://creativecommons.org/publicdomain/zero/1.0/) but we wanted to give credit anyway.

- [Car](Assets/FocusAnalytics/Models/Car) - By MrCraft Animation can be found [here](https://opengameart.org/content/cars-pack).
