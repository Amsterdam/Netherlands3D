# Hoe moet je de Traffic package gebruiken (Dutch)

## Over
De Traffic package kan gebruikt worden voor het volgende:
- Importeren van VISSIM bestanden en de data visualiseren

## Installatie/voorbereiding voor een Unity scene
Voor een scene referentie kunt u de scene "Scene_Traffic_Samples_TrafficSceneSetup" openen die staat onder Traffic > Samples > Traffic Scene Setup > Scenes > Scene_Traffic_Samples_TrafficSceneSetup

### De Traffic Prefab
1. Open een lege Unity scene (of de scene waar je traffic in wilt laten werken)
2. Navigeer naar de prefab "Traffic" in de project window. De prefab kan gevonden worden onder Packages > Netherlands 3D > Traffic > Runtime > Prefabs > Traffic
3. Sleep deze prefab in de scene (of de Hierarchy window)
4. Wanneer er nu een VISSIM bestand word geimporteerd een event word getriggerd waardoor de Traffic prefab het laad en visualiseerd. Je kunt dit zelf testen door play mode te starten, navigeer naar de eringesleepte Traffic prefab, selecteer het gameobject, kijk naar de inspector window van dit object, en klik op de knop "Import .FZP" op het Traffic script om een vissim file te importeren als test.

### De Traffic UI Prefab
1. Om te interacteren met de data een simpele Traffic UI prefab kan in de scene worden gesleept as een kind van de canvas GameObject.
2. Navigeer naar de prefab "Traffic UI" in het project window. De prefab kan gevonden worden onder Packages > Netherlands 3D > Traffic > Runtime > Prefabs > Traffic UI.
3. Sleep dit prefab in de scene als een kind van een Canvas GameObject (Belangrijk!)
4. Je kunt nu interacteren met de Traffic prefab (als die ook in de scene staat)

# How To Use Traffic Package (English)

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