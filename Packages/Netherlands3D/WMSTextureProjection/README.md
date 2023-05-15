WMS Texture Projection
===============

This package contains a TileHandler layer that loads tiles as projected textures from a WMS service.
Supporting scripts allow you to add the layer to an existing TileHandler with LOD steps with different texture sizes for different distances.

Please note that this package depends on the following Netherlands3D packages:

- Core ( TileHandler )

- Rendering ( For the texture projectors )
  
  

Add the CreateWMSLayer to an object in your scene. A TileHandler is required, but does not necessarily need to be on the same object. You can reference the TileHandler in the Optional part of the script. Leaving it empty will cause the script to look up a TileHandler during runtime.

Set 'Projector Prefab' to one of the Projector prefabs in the Prefabs folder of the package:

- DecalProjectorPrefab - Using a Unity URP DecalProjector. This depends on the Decal render feature from the Rendering package.

- RendererProjectorPrefab - Using a custom shader and MeshRenderer. This depends on the stencil rendering features from the Rendering package

Tile Size defaults to 1500. You can change it to the prefered tile size (meters).

The Compress Loaded Textures option defaults to enabled. Disabling will improve performance but increase memory usage.

Wms Lods is an array of different LOD levels for different render distances. The texture size is in pixels. It is recommended to use power-of-two sizes. Make sure the texture size is not larger then the supported output size of your WMS service as it would not increase quality, only texure memory usage.



The Samples contain an example scene showing you this setup as a finished scene. In this example the CreateWMSLayer is placed on the TileHandler object itself.
