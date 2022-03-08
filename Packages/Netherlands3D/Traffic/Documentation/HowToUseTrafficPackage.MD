# How To Use Traffic Package

## About
The traffic 'package' allows the following:
- Importing VISSIM files and visualizing the data

## Installation/setup for a Unity Scene.
For a scene setup reference you can open the scene "Scene_Traffic_Samples_TrafficSceneSetup" located in Traffic > Samples > Traffic Scene Setup > Scenes > Scene_Traffic_Samples_TrafficSceneSetup

### The Traffic Prefab
1. Open an empty scene (or the scene you want the traffic to work in)
2. Navigate to the prefab "Traffic" in the project window. The prefab can be found under Packages > Netherlands 3D > Traffic > Runtime > Prefabs > Traffic
3. Drag this prefab into the scene (or the Hierarchy window)
4. Now when a vissim file is imported an event is triggerd causing Traffic to load it and to visualize it. You can test this yourself by starting play mode, navigate to the dragged in Traffic prefab, selecting this gameobject, look at the inspector of this gameobject, and press the "Import .FZP" button on the script Traffic File Importer Editor to import a vissim file to test.

### The Traffic UI Prefab
1. To interact with the data a simple Traffic UI prefab can be dragged into the scene as that of a child of a canvas gameobject
2. Navigate to the prefab "Traffic UI" in the project window. The prefab can be found under Packages > Netherlands 3D > Traffic > Runtime > Prefabs > Traffic UI
3. Drag this prefab into the scene as child of an Canvas gameobject
4. You can now interact with the Traffic prefab (if that is also in the scene)