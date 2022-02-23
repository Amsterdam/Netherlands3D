# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### [0.0.7] - 23-02-2022

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
