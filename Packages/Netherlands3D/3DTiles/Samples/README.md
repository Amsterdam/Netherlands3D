# 3D Tiles 1.1 TileSets

This example shows how 3D Tiles 1.1 datasets are loaded into view, from url's configured in a json file.

The StreamingAssets folder contains a config.json file where you can add multiple tileset urls in the tilesets array. 

A configuration would look similar to the following:

```json
{
    "viewerVersion": "0.0.1",
    "tilesets": [
        {
            "url": "https://3d.test.kadaster.nl/3dtiles/2020/buildings/tileset.json",
            "maximumScreenSpaceError": 5
        },
        {
            "url": "https://3d.test.kadaster.nl/3dtiles/2020/terrain/tileset.json",
            "maximumScreenSpaceError": 5
        }
    ]
}
```

## The Unity Scene

First, because these tilesets are georeferenced we must determine what the world coordinate of our Unity scene is.
We do this by adding a new GameObject with the SetGlobalRDOrigin script on it, with the world coordinate in RD coordinates.



The Unity scene contains an object called 'ConfigLoader' that reads the StreamingAssets/config.json file and generates a new 'Read3DTileset' object for every tileset url in the config at the start of the scene.
This script deals with downloading the proper tiles into view based on the main camera position, and destroying loaded tiles that move out of the view a.s.a.p to clear up the used memory.



The 'WebTilePrioritiser' is an optional component that the Read3DTileset scripts can use to determine the priority of the tiles that should be loaded based on a max amount of simultanious downloads ( 6 for web ), and the tile position in the screen.

For now the WebTilePrioritiser is the only TilePrioritiser in the package, but programmers can create a new prioritisation system with different logic by extending the TilePrioritiser base class with their own derived Prioritiser.

Depending on the solution, the following parameters can be set in the Inspector:

- SSE Screen Height Limitations (0 is disabled)
  
  - Max Screen Height In Pixels:
    Limits SSE calculations screen resolution to this screen height in pixels.
    This will cause larger 4k screens devices to behave as an HD screen. As a result, larger 4k screen devices will function as if they were an HD screen, which means that the application will not load smaller tiles than on an HD screen. This reduction in tile size decreases the amount of geometry that is loaded, thus decreasing the impact on memory.
  - Max Screen Height In Pixels Mobile:
    If a mobile device is detected, this limit will be utilized using the same logic as the parameter above.

- On Mobile Mode Enabled Unity Event:
  
  - If you need to switch interfaces or activate particular scripts when the TilePrioritizer runs in mobile mode, you can do so by listening to this UnityEvent.

- Web limitations
  
  - Max Simultanious Downloads:
    Many popular web browsers restrict parallel downloads from a single host to 6.
    To ensure that the Tile Prioritizer can effectively prioritize tile loading based on these limitations, this parameter provides the ability to match that download limit. This way, you can optimize your web page's performance and provide a smoother experience for your users.

- Delay tile destroys
  
  - Max Tiles In Dispose List:
    To create a seamless transition between parent and child tiles, our system keeps the meshes of parent tiles in memory until their child tile meshes have loaded. This system uses this parameter as a limit which can be adjusted to optimize your memory usage. If you set the limit to 0, this feature will be disabled and parent meshes will be disposed of immediately. You may want to consider disabling this feature if you are experiencing high memory usage with your tiles.
- Screen space error priority
  - Screen Space Error Score Multiplier:
    This multiplier can increase or decrease the amount of weight that the screen space error of the tiles have when their priority is calculated. The default of 1 uses the SSE as determined by the tileset.
- Center of screen priority
  - Screen Center Score:
    This value represents the maximum score that can be assigned to a tile based on its distance from the 2D center of the screen. By increasing this value, the distance from the center becomes more influential in determining a tile's priority.
  - Screen Center Weight:
    The falloff curve used to distribute tile priority scores based on their 2D position on the screen. 
    For example: A sharp exponential curve will assign significantly higher scores to tiles that are closer to the center of the screen compared to those slightly further away. 
    Adjusting this weight can impact the distribution of scores and ultimately affect which tiles are prioritized for loading.
  - Show Priority Numbers
    By enabling this boolean you enable the numeric labels of tiles in the Editor viewport.

## Editor Gizmos

The editor viewport show helper gizmos if you select the ConfigLoader object.
The colored wireframe cubes show you the tiles that should be loaded into view. The colors show you their state:

- Red: Waiting for priority queue

- Orange: Downloading content

- Green: Downloaded and content visible

The blue lines pointing upwards from the wirecubes visualise the priority for that tile.
Higher lines mean higher priority and will be loaded first. You can use these helpers to configure the properties for the TilePrioritiser.
