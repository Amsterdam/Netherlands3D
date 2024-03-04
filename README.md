## ⚠️ Development Has Moved!

**Attention:** Development for this project has moved to a new location.
Please refer to [github.com/netherlands3d](github.com/netherlands3d) for the latest updates and contributions.

If you have any questions or concerns, feel free to reach out to us.


# Netherlands3D

A library of Unity3D systems/prefabs meant for reuse/addition that were initialy developed for 3D Amsterdam and 3D Utrecht.<br>
This is a clean Unity project with proper settings with the Netherlands3D content living within the [Packages/Netherlands3D](Packages/Netherlands3D) folder.<br>

## Contributing
We encourage other Dutch municipalities to contribute to this package with developments of their own.<br>
Please refer to [CONTRIBUTING.md](CONTRIBUTING.md) for details on how to contribute.

## Using the package
If you like to use (and not contribute to) the Netherlands3D package you can use the Unity Package Manager to install the package:<br>

1. In Unity open 'Window/Package Manager'
2. In the top left of the window, click the plus icon and choose 'Add package from git URL'
3. Use the following URL: https://github.com/Amsterdam/Netherlands3D.git?path=/Packages/Netherlands3D/

## Package specific releases

User can download a specific release version by adding a hash with the tag of the release in the Git package URL.

For example, to download version v.0.0.2 you could use the following URL in the Unity Package Manager:

https://github.com/Amsterdam/Netherlands3D.git?path=/Packages/Netherlands3D/#v0.0.2

The version numbering for the Netherlands3D releases use Semantic Versioning.

## Updating the package

If you imported a Netherlands3D package and like to update the package, or pick another specific version, we recommend doing so via the Netherlands3D menu on top of your Unity editor screen.

We highly recommend picking a specific release version for your project, to make sure the package will stay the same until you deliberately choose to update.

## Running a WebGL Netherlands3D application

If your project uses the TileHandler, and you would like to use the compressed binary tile files (ending in .br) in a WebGL build, you need to make sure your host server (remote or localhost) has the 'Content-Encoding' header set to 'br' for all files that have the .br file extention.

### Local

To develop and test the WebGL locally, it is recommended to build your WebGL output into the Build folder. You can 
either use the "Build and Run" option, or you can start a local webserver that runs the application on 
http://localhost:8080 by running `./bin/start-server.bat` or `./bin/start-server.sh`.

Starting the webserver this way will make it easier to test authentication with the Authentication package because the 
URL is predictable and can be configured as the redirect url when setting up an OAuth app.

### Hosted

The Unity documentation has some example server configurations to get started:
https://docs.unity3d.com/Manual/webgl-server-configuration-code-samples.html

The examples in the Unity documentation are specific to the Unity build files, so you would need to include your binary tile files path yourself.