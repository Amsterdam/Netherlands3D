# 3D Tiles 1.1 TileSets

This example shows how 3D Tiles 1.1 datasets are loaded into view, from url's configured in a json file.

The StreamingAssets folder contains a config.json file where you can add multiple tileset urls in the tilesets array. 

A configuration would look similar to the following:

```C
{
    "viewerVersion": "0.0.1",
    "tilesets": [
        {
            "url": "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/building_1_1/tileset.json",
            "maximumScreenSpaceError": 5
        },
        {
            "url": "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/landuse_1_1/tileset.json",
            "maximumScreenSpaceError": 5
        },
        {
            "url": "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/tiles_1_1/tileset.json",
            "maximumScreenSpaceError": 5            
        }
    ]
}
```

## The Unity Scene

First, because these tilesets are georeferenced we must determine what the world coordinate of our Unity scene is.
We do this by adding a new GameObject with the SetGlobalRDOrigin script on it, with the world coordinate in RD coordinates.



The Unity scene contains a 'ConfigLoader' that reads the StreamingAssets/config.json file and generates a new 'Read3DTileset' object for every tileset url in the config. 
This script deals with downloading the proper tiles into view based on the main camera position, and destroying loaded tiles that move out of the view a.s.a.p to clear up the used memory.



The 'WebTilePrioritiser' is an optional component that the Read3DTileset scripts can use to determine the priority of the tiles that should be loaded based on a max amount of simultanious downloads ( 6 for web ), and the tile position in the screen.

For now the WebTilePrioritiser is the only TilePrioritiser in the package, but programmers can create a new prioritisation system with different logic by extending the TilePrioritiser base class with their own derived Prioritiser.



## Editor Gizmos

The editor viewport shows some gizmos if you select the ConfigLoader object.
The colored wireframe cubes show you the tiles that should be loaded into view. The colors show you their state:

- Red: Waiting for priority queue

- Orange: Downloading content

- Green: Downloaded and content visible

The blue lines pointing upwards from the wirecubes visualise the priority for that tile.
Higher lines mean higher priority and will be loaded first. You can use these helpers to configure the properties for the TilePrioritiser.
