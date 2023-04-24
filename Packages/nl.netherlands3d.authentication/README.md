OAuth Authentication
====================

Installation
------------

Similar to other Netherlands3D packages, this can be installed using the Git URI like this:

1. In Unity open 'Window/Package Manager'
2. In the top left of the window, click the plus icon and choose 'Add package from git URL'
3. Use the following URL: https://github.com/Amsterdam/Netherlands3D.git?path=/Packages/nl.netherlands3d.authentication/

This package depends on the Open Source package https://github.com/cdmvision/authentication-unity; please make sure to 
install it using the same instructions above but with the Package URI 
`https://github.com/cdmvision/authentication-unity.git#1.1.3`.

### WebGL

For WebGL to work, you need to perform additional steps:

1. Import the WebGL sample
2. Move the `js` and `oauth` folder into your `WebGLTemplates` folder
3. Add the following line at the bottom of your `index.html`, right before the closing of the `body` tag:
   ```html
    <script src="js/oauth2-start.js"></script>
   ```

With these changes, the WebGL flow can use the WebGLBrowser and perform the authorization flow in a different way.

Usage
-----

Using this package, you can set up a _session_ with an OAuth-based server. This package includes support for
authentication with:

* Google, 
* Facebook, 
* Github and 
* Azure AD

To do so, create a Scriptable Object asset representing a Session with one of these clients -using the menu 
`Create / ScriptableObjects / Authentication / Session`-, and add the credentials of your OAuth app to it.

> Take note: On the platform of your choice, you need to create an (OAuth) app that can provide you with a client id 
> and client secret. Please see the documentation of your client of choice how to accomplish that.

### Signing in and out

Using the Session, you can sign in, sign out or retrieve user information. By default, the session is 'anonymous'; 
meaning that the user is not authenticated but allowed to interact with the system. After Signing in using the
`SignIn` method, a browser window will open to allow for the user to sign in with their respective provider.

Similarly, the `SignOut` method will close the session and go back to an anonymous session.

> Take note: this doesn't sign you out of the provider's session; meaning that if you sign in again that you will only 
> need to provide a username/password again if the provider intents to. This package does not control that.

### Events

Especially because Signing in is an asynchronous activity, several events have been added to appropriately respond to 
activities in the session.

These are:

- `UnityEvent<IUserInfo> OnSignedIn`
- `UnityEvent OnSignInFailed`
- `UnityEvent OnSignedOut`

All events are present on the `Session` Scriptable Object, and you use the `SessionListener` behaviour to attach 
listeners to these events, or register listeners to them in a script of your own.

### WebGL

During development -when you build your application for WebGL and Desktop-, it is recommended to make sure your
redirect uri in the Session configuration is set to a localhost address with a port above 8000, such as 
`http://localhost:8080/oauth/callback.html`.

With that set, you can start a local webserver that serves your WebGL build on that port, and you can test your 
authorization in all flows the same.

When using python, you could start your local webserver like this:

```bash
python -m http.server 8080 -d ./Build/
```

Where 8080 is the port where it should be served, and `./Build/` is the folder where you have built your WebGL 
distribution to.