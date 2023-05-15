WMS Texture Projection
===============

This package includes a TileHandler layer that loads tiles as projected textures from a WMS service.
Supporting scripts allow you to add the layer to an existing TileHandler with LOD steps, offering different texture sizes for various distances.

Please note that this package depends on the following Netherlands3D packages:

- Core ( TileHandler )

- Rendering ( For the texture projectors )
  
  

To utilize this package, follow these steps:

1. Add the CreateWMSLayer script to an object in your scene. A TileHandler is required but does not necessarily need to be on the same object. You can reference the TileHandler in the Optional section of the script. Leaving it empty will prompt the script to look up a TileHandler during runtime.

2. Set the 'Projector Prefab' to one of the Projector prefabs located in the Prefabs folder of the package:
   
   - DecalProjectorPrefab: Uses a Unity URP DecalProjector. This depends on the Decal render feature from the Rendering package.
   
   - RendererProjectorPrefab: Uses a custom shader and MeshRenderer. This depends on the stencil rendering features from the Rendering package.

3. The default Tile Size is set to 1500. You can modify it to your preferred tile size in meters.

4. By default, the "Compress Loaded Textures" option is enabled. Disabling it will enhance performance but increase memory usage.

5. "Wms Lods" is an array of different LOD levels for various render distances. The texture size is measured in pixels. It is recommended to use power-of-two sizes. Ensure that the texture size does not exceed the supported output size of your WMS service, as it will not improve quality but only increase texture memory usage.

The package also includes a sample scene demonstrating the setup described above. In the example scene, the CreateWMSLayer is placed on the TileHandler object itself.


