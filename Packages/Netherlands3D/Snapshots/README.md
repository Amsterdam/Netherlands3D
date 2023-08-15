Netherlands3D bundle for the Snapshots package
==============================================

## Introduction

The Netherlands3D package uses a series of bundles to tie the individual 
packages together using a Scriptable Object-based eventing system.

This controls the Snapshots package using Netherlands3D's Event System.

## Events

### CreateSnapshot

When invoked, this event will trigger the creation of a snapshot and initiate a Download of the created snapshot.

### SetSnapshotWidth

When invoked, this event will cause newly created snapshots to have this width.

### SetSnapshotHeight

When invoked, this event will cause newly created snapshots to have this height.

### SetSnapshotLayerMasks

When invoked, this event will cause newly created snapshots to only contain elements on a given layer mask.
