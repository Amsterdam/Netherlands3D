mergeInto(LibraryManager.library, 
{
    oAuth: null,
    
    oAuthInit: function()
    {
        this.oAuth = new function() {
            this.client = null;
            this.redirectUri = null;
            this.scope = null;
            this.state = null;
            this.codeVerifier = null;

            this.windowObjectReference = null;
            this.previousUrl = null;

            this.init = () => {
                this.state = this._generateRandomString();

                const script = document.createElement("script");
                script.src = "https://cdn.jsdelivr.net/npm/@badgateway/oauth2-client@2.2.0/browser/oauth2-client.min.js";
                document.head.appendChild(script);

                window.popupCompleted = () => {
                    console.log("Popup completed");
                    console.log(this.windowObjectReference);

                    this.signInSucceeded(this.windowObjectReference.location);
                }
            };
            
            this.setupClient = (authorizationEndpoint, clientId, tokenEndpoint, redirectUri, scope) => {
                const {host, protocol} = new URL(authorizationEndpoint);
                
                this.client = new OAuth2Client.OAuth2Client({
                    server: `${protocol}//${host}`, 
                    clientId, 
                    tokenEndpoint, 
                    authorizationEndpoint
                });
                this.redirectUri = redirectUri;
                this.scope = scope;
            };

            this.signIn = async () => {
                console.log("signing in");
                this.codeVerifier = await OAuth2Client.generateCodeVerifier();
                const authorizeUri = await this.client.authorizationCode.getAuthorizeUri({
                    redirectUri: this.redirectUri, 
                    state: this.state, 
                    codeVerifier: this.codeVerifier, 
                    scope: this.scope
                });

                this._openSignInWindow(authorizeUri, 'myOAuth');
            };
            
            this.signInSucceeded = async (codeRedirect) =>
            {
                console.log("sign in succeeded");
                console.log(codeRedirect);
                console.log(codeRedirect.toString());
                const token = await this.client.authorizationCode.getTokenFromCodeRedirect(
                    codeRedirect.toString(),
                    {
                        redirectUri: this.redirectUri,
                        state: this.state,
                        codeVerifier: this.codeVerifier,
                    }
                );
                console.log(token);

                unityInstance.SendMessage("WebGlResponseInterceptor", "SignedIn", JSON.stringify(token));
                // unityInstance.SendMessage("WebGlResponseInterceptor", "SignInFailed");
            };

            this._generateRandomString = () => {
                return Math.floor(Math.random() * Date.now()).toString(36);
            };

            this._openSignInWindow = (url, name) => {
                // window.removeEventListener('message', this._receiveMessage);

                const strWindowFeatures = 'toolbar=no, menubar=no, width=600, height=700, top=100, left=100';

                if (this.windowObjectReference === null || this.windowObjectReference.closed) {
                    this.windowObjectReference = window.open(url, name, strWindowFeatures);
                } else if (this.previousUrl !== url) {
                    this.windowObjectReference = window.open(url, name, strWindowFeatures);
                    this.windowObjectReference.focus();
                } else {
                    this.windowObjectReference.focus();
                }

                // window.addEventListener('message', event => this._receiveMessage(event), false);
                this.previousUrl = url;
            };

            // this._receiveMessage = (e) => {
            //     const { origin, data } = e;
            //     console.log(origin);
            //     console.log(data);
            //     // Do we trust the sender of this message? (might be
            //     // different from what we originally opened, for example).
            //    
            //     // if (origin !== BASE_URL) {
            //     //     return;
            //     // }
            //    
            //     // if we trust the sender and the source is our popup
            //     if (data.source === 'lma-login-redirect') {
            //         // get the URL params and redirect to our server to use Passport to auth/login
            //         const { payload } = data;
            //         const redirectUrl = `/auth/google/login${payload}`;
            //         window.location.pathname = redirectUrl;
            //     }
            // }

            this.init();
        };
    },
    
    oAuthSignIn: async function (authorizationEndpoint, tokenEndpoint, clientId, clientSecret, redirectUri, scopes) {
        authorizationEndpoint = UTF8ToString(authorizationEndpoint);
        redirectUri = UTF8ToString(redirectUri);
        clientId = UTF8ToString(clientId);
        tokenEndpoint = UTF8ToString(tokenEndpoint);
        const scope = UTF8ToString(scopes).split(' ');
        
        this.oAuth.setupClient(authorizationEndpoint, clientId, tokenEndpoint, redirectUri, scope);
        this.oAuth.signIn();
    },
    
    oAuthSignOut: function() {
        // TODO: Send a signal to the js library
    },

});