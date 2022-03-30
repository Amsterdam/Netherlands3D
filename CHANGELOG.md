# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- moved all the shaders into their corresponding samples, when updating, make sure to open the samples and copy the shaders into your assets-folder.
- separated Input, BAGInformation, DataParsing and Interface into their own root folders and script assemblies

### Added
- added improved UI to WMS-Layer Sample 
- added ADD_BROTLI_ACCEPT_ENCODING_HEADER scripting define symbol to enable adding the Accept-Encoding header for brotli files
- added scripts to retrieve GeoJSON properties, and an example using pdok.nl API's to retrieve BAG information after clicking on a building
- added basic modular interface scripts to show connections with an interface using events

### [0.0.8] - 23-02-2022

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
