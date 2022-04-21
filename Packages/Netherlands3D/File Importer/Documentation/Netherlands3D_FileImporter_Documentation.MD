# Netherlands3D File Importer Documentation

## About
The File Importer package allows for importing files into unity.
The current platforms that are supported are:
- Windows
- Mac
- Linux
- WebGL

## How To Use
0. Each scene that uses the `ImportFileButtonOnClick` component should have the prefab `UserFileUploads`. This name cannot be changed and the prefab should only be dragged into the scene. The prefab is located in FileImporter > Runtime > Prefabs > UserFileUploads
1. Attach the component "ImportFileButtonOnClick" to a gameobject where a button component is attached too.
2. Set the fileInputName to that of your liking
3. Set the file extention the file has to be, leaving it empty allows for all files.
4. Set the multiSelect to your liking (allowing for multiple files to be imported)
5. Press play and test the button. Based on the development enviroment the button will show an import window to select files.
6. Once a file is getting imported the `StringEvent` eventFileLoaderFileImported will be triggerd and you can use this callback to read the file. (It contains the path(s) to the file(s))

## Credit
- Standalone File Browser @ Copyright (c) 2017 Gökhan Gökçe

## Notes
- Put the Player > Configuration > Assembly version validation off (had .net form error)