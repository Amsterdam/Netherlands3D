mergeInto(LibraryManager.library, 
{
    client: null,

    oAuthInit: function()
    {
        var script = document.createElement("script");
        script.src = "https://unpkg.com/jso/dist/jso.js";
        document.head.appendChild(script);
    },

    oAuthSignIn: function (authorizationEndpoint, tokenEndpoint, clientId, clientSecret, redirectUri, scopes) 
    {
        let config = {
            client_id: UTF8ToString(clientId),
            redirect_uri: UTF8ToString(redirectUri),
            authorization: UTF8ToString(authorizationEndpoint),
            scopes: { request: UTF8ToString(scopes).split(' ')},
            default_lifetime: 3600,
        };

        // use Authorization Code Flow instead of Implicit Flow
        if (clientSecret) {
            config.response_type = "code";
            config.client_secret = UTF8ToString(clientSecret);
            config.token = UTF8ToString(tokenEndpoint);
        }

        this.client = new jso.JSO(config);
        this.client.setLoader(jso.Popup);
        this.client.getToken({ redirect_uri: "http://localhost:8080/oauth/callback.html" })
            .then((token) => {
                unityInstance.SendMessage("WebGlResponseInterceptor", "SignedIn", JSON.stringify(token));
            })
            .catch((err) => {
                console.error("Error while fetching OAuth token", err)
                unityInstance.SendMessage("WebGlResponseInterceptor", "SignInFailed");
            })
    },
    
    oAuthSignOut: function() {
        this.client.wipeTokens();
    }
});