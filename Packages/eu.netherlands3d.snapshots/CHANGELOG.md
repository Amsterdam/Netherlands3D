# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Snapshot class with the following features:
  - ToImageBytes
  - ToTexture2D
  - ToRenderTexture
- Properties and serialized fields for various settings in the Snapshots MonoBehaviour

### Changed

- Snapshot settings in Snapshots can now be edited in the Editor
- Snapshots taken on a platform other than WebGL and the Editor will be placed in the Application.persistentDataPath 
  folder, using the 'targetPath' field as subfolder specification.
- Default value for the targetPath field was changed to "screenshots".

### Fixed

- Main camera was always used to take screenshots, even when a different camera was set
- Saving a screenshot silently failed on all platforms except WebGL and Editor

### Deprecated

- Snapshots.SetLayerMask() - replaced by Snapshots.SnapshotLayers
- Snapshots.SetImageWidth() - replaced by Snapshots.Width
- Snapshots.SetImageHeight() - replaced by Snapshots.Height
- Snapshots.SnapshotToImageBytes() - replaced by Snapshot.ToImageBytes()