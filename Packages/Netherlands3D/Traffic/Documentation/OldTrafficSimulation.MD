# Documentation of old simulation generation Traffic
Drag selection grid, places cars on the roads
doenst really show cars, maybe create a systemlike gta cars

## UI
- button generate traffic
GridselectionTool.StartSelection(TrafficBlock)
Traffic.GenerateRoads.WaitForGridBounds
Helpmessage.show(<b>Shift+Klik+Sleep</b> om een verkeer gebied te selecteren)

## Scripts

## TrafficSimulator -> Traffic.Simulation? Simulator
Main class
- PlaceCar(RoadObject) : Void
// Places a car on a roadobject
- GenerateVehicleType(float): GameObject
// Returns a vehicle gameobject
- SimulateInBounds(Bounds)
// turns off vehicles when they travel outside the bounds
- StartSimulation(bool) : void
// Enables/disables vehicles from driving
- UpdateVehicleSpeed(int) : void
// updates the speed of all vehicles

## GenerateRoads
- roadObject: GameObject
- WaitForGridBounds() : void
// subscribes to onGridSelected
- StopWaitingForGridBounds() : void
// Removes listener
- ShowTraffic(Bounds) : void
// Shows traffic in a bound & then starts simulation
- StartSimulation() : void
// stops and then starts the simulation of TrafficSimulator
- Update() : void
// some fuckery with frames/shuffeling
- GetRoadsJson() : IEnumerator
// Retrieves road json from open street maps
- StartRoadGeneration(string): void
// Processes the json data
- GenerateRoad(JSONNode)
// Generates a road based on id

## ChangeTrafficSettingsUI
// handles ui related values

## RoadObject
// Parent of roadpoints, placed wayyy the fuck out causing float pointer errorss
// Takes jsonnode data and converts it into road points
- CreateRoad(JSONNODE): void

## RoadPoint
// A transform placed in the world with coordinates from irl world coords

## RoadsJson
// empty class containing classes for json data

## Vehicle
// that drives on a roadobject, fuckery with driving on frames
- Start(): void
- ApplyCarSettings(): void // sets color
- GenerateColor(float): void // creates a random color
- Update(): void // big spagetti mess of driving the car to next point
- MoveCar(Vector3): void // moves the car and looks for new roads if it needs it
- SetTerrainLayer(BinaryMeshLayer): void // sets layer

## VechicleProperties
// For placing the wheels correctly
