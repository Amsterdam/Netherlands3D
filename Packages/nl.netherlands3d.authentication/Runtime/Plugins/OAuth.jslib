mergeInto(LibraryManager.library, 
{
    client: null,

    oAuthInit: function()
    {
        var script = document.createElement("script");
        script.src = "https://cdn.jsdelivr.net/npm/@badgateway/oauth2-client@2.2.0/browser/oauth2-client.min.js";
        document.head.appendChild(script);
    },

    oAuthSignIn: async function (authorizationEndpoint, tokenEndpoint, clientId, clientSecret, redirectUri, scopes) {
        const client = new OAuth2Client({
            server: '',
            clientId: UTF8ToString(clientId),
            tokenEndpoint: UTF8ToString(tokenEndpoint),
            authorizationEndpoint: UTF8ToString(authorizationEndpoint),
        });

        const codeVerifier = await generateCodeVerifier();

        document.location = await client.authorizationCode.getAuthorizeUri({
            redirectUri: UTF8ToString(redirectUri),
            state: 'some-string',
            codeVerifier,
            scope: UTF8ToString(scopes).split(' '),

        });

        // unityInstance.SendMessage("WebGlResponseInterceptor", "SignedIn", JSON.stringify(token));
        // unityInstance.SendMessage("WebGlResponseInterceptor", "SignInFailed");
    },
    
    oAuthSignOut: function() {
    }
});