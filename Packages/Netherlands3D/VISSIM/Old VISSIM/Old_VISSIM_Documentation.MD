# Old VISSIM Documentation to understand how it works / Code layout
- Got Vissim and traffic simulator

## Gamelogic flow
1. Loads .fzp file <- triggerd by event
2. converts .fzp file to vissim data
3. creates matching vehicles in world
4. updates vehicles

## In Scene 3D Utrecht vissim gameobject
fileparsing > ImportVissim
- VissimStringLoader
- ConvertFZP
- Playback
- VissimConfiguration

## VissimStringLoader, loads string file and converts data to visual
- FileImported()
- editor LoadVissimViaEditor
- LoadVissimFromFile
- LoadingProgress
- DestroyVissim

## ConvertFZP, fzp is file format type
- missingVissimTypes: List<int> = if vehicleType isnt there
- vehicleTypes: Dictonary<int, GameObject[]>
- vissimScriptableObjects (car/truck/etc)
- ReadFileFZP(text): void
- AddVissimData(dataString): void
- SortDataByCar(VissimData)
- ConvertStringToVector(strinVector): void
- RetrieveVissim(apiUrl): IEnumerator
- LoadDefaultVissimData(): void
- StartVissim(): void

## Playback, plays back all vehicles after ConvertFZP has finishedLoadingData
- Update(): void
- SendCommand(List<VissimData>): void = Checks the current simulation time and sends according commands to all vehicles.
- SendCameraToVissim()
- SendCameraToStart()

## VissimConfiguration, has to do something with UI
- fileConverter fzp
- list of UI
- prefabs
- OpenInterface(List<int> missingVehiclesTypes): void = Opens missing Vissim files interface
- SetVehicles(): void

## VissimManager
- not really a manager
- vehicleTypes<>
- convertStringToVector()
- contains test string data

## VissimData, Contains input data in class form
- class to contain serializable data extracted from strings

## VissimVehicle, base class for all vehicles = OPTIMAZATION
- Update(): void = used to cleanup vehicle
- LateUpdate(): void = used for rotating car
- MoveAnimation(startPos:V3, endPos:V3, animationTime:float): virutal void = move vehicle trough animation
- CleanUpVehicle(): virtual void = sets objects inactive
- SetMeshLayer(BinaryMeshLayer layer): virtual void

## Pedestrian : VissimVehicle, 

# VissimType, scriptable object containing type of vehicle?