# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.0.0] - 25-03-2024

### Removed

- Removed legacy packages that are moved, or will be moved to dedicated repo packages at github.com/netherlands3d

## [3.2.0] - 04-03-2024

### Removed

- Removed Traffic. Traffic package is now available seperately on github.com/netherlands3d/traffic

### Added

- Manual for WMS layer example
- Added minimap example template using PDOK maps
- WMS layer LOD and distance setting available in editor inspector
- EventContainers with basic payload types can now be invoked from the asset in inspector during playmode
- Added script to find and count unique instances of ScriptableObjects in the scene (available in rightclick context menu in the project page)

### Changed

- Invariant Culture is now set globally using SetGlobalRDOrigin (for consistent parsing of comma's or dots in large numbers)
- Camera input script is now on FreeCamera prefab (instead of seperate object) and legacy input is removed

### Fixed

- BAG building selection example now properly uses new InputSystem for its selection input

## [0.3.0] - 12-07-2022

- Added Traffic package with VISSIM traffic and signal head import and visualisation
- Fixed version numbering for in-editor updates

## [0.2.0] - 15-06-2022

### Changed

- TileHandler sample scene now uses .br instead of .unityweb as the default brotli compressed extention instead. ( matches TileBakeTool default brotli extention )

### Fixed

- in webGL with brotli-compression for the binary-tiles the accompanying data-files could not be found.
- GeoJSON parser now uses correct 'MultiPolygon' lookup string, solving a problem where parsing MultiPolygons would return the first 'Polygon' geometry occurance

### Added

- Added Timeline interface for scrubbing through time-bound events

## [0.1.1] - 17-05-2022

### Added

- [VersionControl] added button to import selected version

## [0.1.0] - 17-05-2022

### Changed

- moved all the shaders into their corresponding samples, when updating, make sure to open the samples and copy the shaders into your assets-folder.
- separated Input, BAGInformation, DataParsing and Interface into their own root folders and script assemblies

### Added

- added versionControl for package
- added subobject filtering code and example to filter objects based on geojson float values (construction year in example)
- added sun/shadows based on location, date and time scripts and example
- added camera based on 3DAmsterdam and 3DUtrecht behavior
- added improved UI to WMS-Layer Sample 
- added ADD_BROTLI_ACCEPT_ENCODING_HEADER scripting define symbol to enable adding the Accept-Encoding header for brotli files
- added scripts to retrieve GeoJSON properties, and an example using pdok.nl API's to retrieve BAG information after clicking on a building
- added basic modular interface scripts to show connections with an interface using events

## [0.0.8] - 23-02-2022

### Changed

- Keep track of SubObjects coroutine so it can be interupted before calling a new coloring process

## [0.0.7] - 23-02-2022

### Added

- Added WMS-Layer
- Added SampleScene with tooling to find wms-url's for use in the WMS-layer

## [0.0.6] - 21-02-2022

### Added

- Added ColorPalette and GradientContainer events for passing their ScriptableObject data
- Added listener to ColorSubObjects for setting GradientContainer objects

## [0.0.5] - 21-02-2022

### Removed

- Removed GeoService from package untill feature is complete
- Removed empty TrafficSimulation folder

## [0.0.4] - 20-02-2022

### Added

- Removed nested empty readme and license duplicates
- Added new event in ColorSubObjects to allow clearing data from memory

## [0.0.3] - 17-02-2022

### Added

- Added new event with GameObject as payload type
- Added TextMeshPro 3.0.6 as package dependency because it is required by an example
- Added correct path to repo CHANGELOG.MD file in package.json

## [0.0.2] - 10-02-2022

### Added

- Added changelog file
- Added gradient and color palette scriptable object containers for reusing colors and gradients
- Added GeoService projector system

### Changed

- Increased version number

### Removed

- Removed nested package.json files

## [0.0.1] - 09-02-2022

### Added

- Initial release
